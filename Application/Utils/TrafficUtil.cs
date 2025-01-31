using Microsoft.EntityFrameworkCore;
using MTWireGuard.Application.Models;
using MTWireGuard.Application.Repositories;
using Serilog;

namespace MTWireGuard.Application.Utils
{
    public class TrafficUtil
    {
        public static List<UsageObject> ParseTrafficUsage(string input)
        {
            string[] items = input.Split(".id=");

            List<string> objects = items
                .Where(item => !string.IsNullOrEmpty(item))
                .Select(item => $"id={item}")
                .ToList();

            return objects
                .Select(x => x.Split(';').ToList())
                .Select(arr =>
                {
                    var obj = new UsageObject();
                    var id = arr.Find(x => x.Contains("id")).Split('=')[1];
                    var rx = arr.Find(x => x.Contains("rx")).Split('=')[1] ?? "0";
                    var tx = arr.Find(x => x.Contains("tx")).Split('=')[1] ?? "0";
                    obj.Id = id;
                    obj.RX = ulong.Parse(rx);
                    obj.TX = ulong.Parse(tx);
                    return obj;
                }).ToList();
        }

        public static async void HandleUserTraffics(List<DataUsage> updates, DBContext dbContext, IMikrotikRepository API, ILogger logger)
        {
            var dataUsages = await dbContext.DataUsages.ToListAsync();
            var existingItems = dataUsages.OrderBy(x => x.CreationTime).ToList();
            var lastKnownTraffics = dbContext.LastKnownTraffic.ToList();
            var users = await dbContext.Users.ToListAsync();

            foreach (var item in updates)
            {
                var tempUser = users.Find(x => x.Id == item.UserID);
                if (tempUser == null) continue;

                using var transactionDbContext = new DBContext(); // Create a new DbContext for each transaction
                using var transaction = await transactionDbContext.Database.BeginTransactionAsync();
                try
                {
                    LastKnownTraffic lastKnown = lastKnownTraffics.Find(x => x.UserID == item.UserID);
                    if (lastKnown == null) continue;

                    var old = existingItems.FindLast(oldItem => oldItem.UserID == item.UserID);
                    if (old == null)
                    {
                        await transactionDbContext.DataUsages.AddAsync(item);
                        tempUser.RX = item.RX + lastKnown.RX;
                        tempUser.TX = item.TX + lastKnown.TX;
                    }
                    else
                    {
                        if ((old.RX <= item.RX || old.TX <= item.TX) &&
                            (old.RX != item.RX && old.TX != item.TX)) // Normal Data (and not duplicate)
                        {
                            await transactionDbContext.DataUsages.AddAsync(item);
                        }
                        else if (old.RX > item.RX || old.TX > item.TX) // Server Reset
                        {
                            lastKnown.RX = old.RX;
                            lastKnown.TX = old.TX;
                            lastKnown.CreationTime = DateTime.Now;
                            transactionDbContext.LastKnownTraffic.Update(lastKnown);
                            item.ResetNotes = $"System reset detected at: {DateTime.Now}";
                            await transactionDbContext.DataUsages.AddAsync(item);
                        }
                        if (item.RX > old.RX) tempUser.RX = item.RX + lastKnown.RX;
                        if (item.TX > old.TX) tempUser.TX = item.TX + lastKnown.TX;
                    }
                    if (tempUser.TrafficLimit > 0 && tempUser.RX + tempUser.TX >= ConverterUtil.GigabyteToByte(tempUser.TrafficLimit))
                    {
                        // Disable User
                        logger.Information($"User #{tempUser.Id} reached {tempUser.RX + tempUser.TX} of {ConverterUtil.GigabyteToByte(tempUser.TrafficLimit)} bandwidth.");
                        var disable = await API.DisableUser(item.UserID);
                        if (disable.Code != "200")
                        {
                            logger.Error("Failed disabling user", new
                            {
                                userId = item.UserID,
                                disable.Code,
                                disable.Title,
                                disable.Description
                            });
                        }
                        else
                        {
                            logger.Information("Disabled user due to bandwidth limit", new
                            {
                                item.UserID,
                                TrafficUsed = ConverterUtil.ConvertByteSize(tempUser.RX + tempUser.TX),
                                tempUser.TrafficLimit
                            });
                        }
                    }
                    transactionDbContext.Users.Update(tempUser);
                    await transactionDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (DbUpdateException ex)
                {
                    logger.Error(ex.Message);
                    await transaction.RollbackAsync();
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    await transaction.RollbackAsync();
                }
            }
        }
    }
}
