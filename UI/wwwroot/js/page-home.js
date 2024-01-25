const assetsPath = "../assets/";
const api = new APIClient();

let renderTableCell = (function () {
  function renderName(full) {
    var $name = full['name'],
      $lastSeen = full['lastHandshake'];

    // Avatar badge
    var stateNum = Math.floor(Math.random() * 6);
    var states = ['success', 'danger', 'warning', 'info', 'dark', 'primary', 'secondary'];
    var $state = states[stateNum],
      $name = full['name'],
      $initials = ($name || '').match(/\b\w/g)?.join('').toUpperCase() || '';
    let $output = '<div class="position-relative badge rounded-circle h-100 w-100 d-flex justify-content-center align-items-center bg-' + $state + '">' + $initials + '</div>';

    // Creates full output for row
    var $row_output =
      '<div class="d-flex justify-content-center align-items-center">' +
      '<div class="avatar-wrapper">' +
      '<div class="avatar">' +
      $output +
      '</div>' +
      '</div>' +
      '</div>';
    return $row_output;
  }

  return {
    renderName
  }
})();

$(function() {
  'use strict';

  var dt_basic_table = $('.datatables-basic');

  // DataTable with buttons
  // --------------------------------------------------------------------

  if (dt_basic_table.length) {
    try {
      var dt_basic = dt_basic_table.DataTable({
        ajax: function(data, callback, settings) {
          settings.sAjaxDataProp = '';
          api.users.getAll()
            .then(function(users) {
              let lastOnline = users.filter(u => u.lastHandshake.length == 8).sort((a, b) => {
                if (a.lastHandshake < b.lastHandshake) {
                  return -1;
                }
              }).slice(0, 5);
              setUsersCount(users);
              callback(lastOnline);
            })
            .catch(function(error) {
              console.log(error);
              callback([]);
            });     
        },
        buttons: null,
        columns: [
          { data: 'name' },
          { data: 'name' },
          { data: 'lastHandshake' }
        ],
        columnDefs: [
          {
            // Avatar
            targets: 0,
            render: function(data, type, full, meta) {
              return renderTableCell.renderName(full);
            }
          },
          {
            // Name
            responsivePriority: 1,
            targets: 1
          },
          {
            // Last Seen
            responsivePriority: 2,
            targets: 2,
            render: function(data, type, full, meta) {
              return `${data}<br>ago`;
            }
          }
        ],
        order: [[2, "asc"]],
        ordering: false,
        dom: 'lrtip',
        lengthChange: false,
        info: false,
        paging: false,
        language: {
          "emptyTable": "No users online today"
        },
        initComplete: function(settings, json) {
          api.config.information.get().then(info => {
            let regexParentheses = /\(([^)]+)\)/;
            let ipInfo = getIPInfo(info.ip);
            let isp = (ipInfo.org ?? ipInfo.isp) ?? `N/A`;
            let countryBadge = ipInfo ? `${ipInfo.country} - ${isp}` : `N/A`;
            $('#info-identity').text(info.identity);
            $('#info-device').html(`${info.device.boardName} <span class="badge border border-purple color-purple p-1">${info.device.architecture}</span>`);
            $('#info-version').html(`${info.version.split(' ')[0]} <span class="badge border border-yellow color-yellow p-1">${regexParentheses.exec(info.version)[1]}</span>`);
            $('#info-ip').html(`${info.ip} <span class="badge border border-info text-info p-1">${countryBadge}</span>`);
            $('#info-dns').text(info.dns.join(' - '));
          });

          api.servers.getAll().then(servers => {
            let activeServers = servers.filter(s => s.isEnabled === true);
            $('#servers-total').html(servers.length.toString());
            $('#servers-active').html(activeServers.length.toString());
          });
        }
      });
    }
    catch(err) {
      console.error(err);
    }
  }

  function setUsersCount(users) {
    let onlineUsers = users.filter(u => u.lastHandshake.length == 8);
    onlineUsers = onlineUsers.filter(u => new Date(`2000-01-01T${u.lastHandshake}`) < new Date(`2000-01-01T00:03:00`));
    $('#users-total').html(users.length.toString());
    $('#users-online').html(onlineUsers.length.toString());
  }
});
