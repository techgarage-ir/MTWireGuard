const assetsPath = "../assets/";
const api = new APIClient();
const toastContainer = $('.toast-container');

let renderTableCell = (function () {
  function renderTitle(title) {
    return title.replace('_', ' ').replace(/(\b\w)/g, char => char.toUpperCase())
      .replace('Ip', 'IP ')
      .replace('Dns', 'DNS ')
      .replace('Mtu', 'MTU');
  }

  function renderSwitch(data, disable=false) {
    let enabled = data == true;
    let active = enabled ? ' active' : '';
    let disabled = disable ? 'disabled' : '';
    return '    <button type="button" class="btn btn-sm btn-toggle item-activation' + active + '" data-bs-toggle="button" aria-pressed="' + enabled + '" autocomplete="off"' + disabled + '>' +
    '      <div class="handle"></div>' +
    '    </button>';
  }

  function renderBadge(data) {
    return '<span class="badge text-bg-warning text-uppercase">' + data + '</span>';
  }

  return {
    renderTitle,
    renderSwitch,
    renderBadge
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
          api.servers.getAll()
            .then(function(response) {
              callback(response);
            })
            .catch(function(error) {
              console.error(error);
              callback([]);
            });
        },
        columns: [
          { data: '' },
          { data: 'isEnabled' },
          { data: 'name' },
          { data: 'ipAddress' },
          { data: 'listenPort'},
          { data: 'ipPool' },
          { data: 'dnsAddress' },
          { data: '' }
        ],
        columnDefs: [
          {
            // For Responsive
            targets: 0,
            className: 'dtr-control',
            orderable: false,
            responsivePriority: 2,
            searchable: false,
            render: function(data, type, full, meta) {
              return '';
            }
          },
          {
            // Enablity Switch
            targets: 1,
            orderable: false,
            searchable: false,
            render: function(data, type, full, meta) {
                return renderTableCell.renderSwitch(data);
            }
          },
          {
            // Name and Online status
            targets: 2,
            responsivePriority: 1
          },
          {
            // IP Address
            responsivePriority: 3,
            targets: 3
          },
          {
            // Port
            responsivePriority: 4,
            targets: 4
          },
          {
            // IP Pool
            targets: 5
          },
          {
            // DNS
            targets: 6,
            render: function(data, type, full, meta) {
              return renderTableCell.renderBadge(data);
            }
          },
          {
            // Actions
            targets: -1, // Last
            title: 'Actions',
            orderable: false,
            searchable: false,
            className: 'action-buttons',
            render: function(data, type, full, meta) {
              return (
                '<a href="javascript:;" class="btn text-primary btn-icon item-edit" data-bs-toggle="offcanvas" data-bs-target="#offcanvasEditServer"><i class="bx bxs-edit"></i></a>' +
                '<a href="javascript:;" class="btn text-primary btn-icon item-details"><i class="bx bx-detail"></i></a>' +
                '<a href="javascript:;" class="btn text-danger btn-icon item-delete" data-bs-toggle="modal" data-bs-target="#RemoveModal"><i class="bx bx-trash"></i></a>'
              );
            }
          }
        ],
        order: [[2, 'desc']],
        dom: '<"card-header"<"dt-action-buttons text-end"B>><"d-flex justify-content-between align-items-center row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>t<"d-flex justify-content-between row"<"col-sm-12 col-md-6"i><"col-sm-12 col-md-6"p>>',
        displayLength: 10,
        lengthMenu: [5, 10, 25, 50, 75, 100],
        buttons: {
          dom: {
            button: {
              tag: 'button',
              className: 'btn'
            }
          },
          buttons: [
            {
              extend: 'collection',
              className: 'btn-primary dropdown-toggle me-2',
              text: '<i class="bx bx-show me-1"></i>Export',
              buttons: [
                {
                  extend: 'copy',
                  text: '<i class="bx bx-copy me-1"></i>Copy',
                  className: 'dropdown-item'
                },
                {
                  extend: 'csv',
                  text: '<i class="bx bx-file me-1"></i>CSV',
                  className: 'dropdown-item',
                  exportOptions: { columns: [3, 4, 5, 6, 7] }
                },
                {
                  text: '<i class="bx bxs-file-json me-1"></i>JSON',
                  className: 'dropdown-item',
                  action: function ( e, dt, button, config ) {
                    let array = [];
                    let data = dt.rows().data();
                    $.each(data, function(i) {
                      array.push(data[i]);
                    });

                    $.fn.dataTable.fileSave(
                        new Blob([JSON.stringify(array, null, 2)]),
                        'wg-servers.json'
                    );
                  }
                },
                {
                  text: '<i class="bx bx-hdd me-1"></i>Backup',
                  className: 'dropdown-item'
                }
              ]
            },
            {
              text: '<i class="bx bx-plus me-1"></i> <span class="d-none d-lg-inline-block">Add New Server</span>',
              className: 'btn-success create-new',
              attr: {
                "data-bs-toggle": "offcanvas",
                "data-bs-target": "#offcanvasAddServer"
              }
            }
          ]
        },
        responsive: {
          details: {
            display: $.fn.dataTable.Responsive.display.modal({
              header: function(row) {
                var data = row.data();
                return 'Details of ' + data['name'];
              }
            }),
            type: 'column',
            renderer: function(api, rowIdx, columns) {
              var data = $.map(columns, function(col, i) {
                return col.title !== '' // ? Do not show row in modal popup if title is blank (for check box)
                  ? '<tr data-dt-row="' +
                      col.rowIndex +
                      '" data-dt-column="' +
                      col.columnIndex +
                      '">' +
                      '<td>' +
                      col.title +
                      ':' +
                      '</td> ' +
                      '<td>' +
                      col.data +
                      '</td>' +
                      '</tr>'
                  : '';
              }).join('');
              let output = data ? $('<table class="table"/><tbody />').append(data) : false;
  
              return output;
            }
          }
        },
        language: {
          "emptyTable": "No servers found"
        },
        initComplete: function(settings, json) {
          const result = Object.values(json).reduce(
            (acc, currentItem) => {
              acc.totalItems++;
              if (currentItem.isEnabled) {
                acc.activeItems++;
              }
              if (currentItem.running) {
                acc.runningItems++;
              } else {
                acc.notRunningItems++;
              }
              return acc;
            },
            { totalItems: 0, activeItems: 0, runningItems: 0, notRunningItems: 0 }
          );
          const totalItems = result.totalItems;
          const activeItems = result.activeItems;
          const runningItems = result.runningItems;
          const notRunningItems = result.notRunningItems;
          $('#servers-total').text(totalItems);
          $('#servers-active').text(activeItems);
          $('#servers-running').text(runningItems);
          $('#servers-not-running').text(notRunningItems);
          
          dt_basic.on('click', 'a.item-details', function (e) {
            let tr = e.target.closest('tr');
            let row = dt_basic.row(tr)
            let rowData = row.data();
            var data = $.map(rowData, function(value, key) {
              if (key !== '') // ? Do not show row in modal popup if title is blank (for check box)
                {
                  let tdValue = '';
                  switch (key) {
                    case 'enabled':
                      tdValue = renderTableCell.renderSwitch(value, true);
                      break;
                    case 'dns_address':
                      tdValue = renderTableCell.renderBadge(value);
                      break;
                    default:
                      tdValue = value;
                      break;
                  }
                  return '<tr>' +
                    '<td>' +
                    renderTableCell.renderTitle(key) +
                    '</td> ' +
                    '<td>' +
                      tdValue +
                    '</td>' +
                  '</tr>'
                }
              else {
                return '';
              }
            }).join('');
            
            let body = data ? $('<table class="table"/><tbody />').append(data) : false;
            $('#DetailsModal .modal-body').html(body);
            $('#DetailsModal .modal-title').html('Details of ' + row.data()["name"]);
            const detailsModal = new bootstrap.Modal('#DetailsModal');
            detailsModal.show();
          });
          dt_basic.on('click', 'a.item-edit', function (e) {
            let tr = e.target.closest('tr');
            let row = dt_basic.row(tr)
            let rowData = row.data();
            
            $.map(rowData, function(value, key) {
              key = key.replace('_', '-');
              let field = $('#offcanvasEditServer #edit-' + key);
              if (key == 'inheritDNS' || key == 'useIPPool')
                console.log(`${key}\t => ${value}`);
              switch (key) {
                case 'ipAddress':
                  let ipAddress = value.split('/')[0];
                  let ipRange   = value.split('/')[1];
                  $('#offcanvasEditServer #edit-ip-address').val(ipAddress);
                  $('#offcanvasEditServer #edit-ip-cidr').val(ipRange);
                  break;
                case 'inheritDNS':
                  $('#offcanvasEditServer #edit-dns-dynamic').prop('checked', value);
                  $('#offcanvasEditServer #edit-dns-server').prop('disabled', value);
                  break;
                case 'dnsAddress':
                  $('#offcanvasEditServer #edit-dns-server').val(value);
                  break;
                case 'useIPPool':
                  $('#offcanvasEditServer #edit-static-addressing').prop('checked', !value);
                  $('#offcanvasEditServer #edit-pool').prop('disabled', !value);
                break;
                case 'ipPool':
                  $(`#offcanvasEditServer #edit-pool option[data-value="${value}"]`).prop("selected", true);
                break;
                case 'listenPort':
                  $('#offcanvasEditServer #edit-port').val(value);
                  break;
                case 'privateKey':
                  $('#offcanvasEditServer #edit-private-key').val(value);
                  break;
                  case 'publicKey':
                    $('#offcanvasEditServer #edit-public-key').val(value);
                    break;
                default:
                  field.val(value);
                  break;
              }
            });
          });
          dt_basic.on('click', 'a.item-delete', function (e) {
            let tr = e.target.closest('tr');
            let row = dt_basic.row(tr)
            let rowData = row.data();
            let serverId = rowData['id'];
            $('#removeForm input[name="ID"]').val(serverId);
          });
          dt_basic.on('click', 'button.item-activation', function (e) {
            let tr = e.target.closest('tr');
            let row = dt_basic.row(tr)
            let rowData = row.data();
            let serverId = rowData['id'];
            let enabled = rowData['isEnabled'];
            console.log(`${serverId}: ${enabled}`);
            api.servers.activate(serverId, {enabled: enabled}).then(data => {
              const toastMSG = new toastMessage("Activate Server", data.body, data.title, data.background);
              const toastElement = toastMSG.getElement();
              toastContainer.append(toastElement);
              bootstrap.Toast.getOrCreateInstance(toastElement).show();
            }).catch(err => {
              console.error(err);
            }).finally(() => {
              dt_basic.ajax.reload();
            });
          });
        }
      });
    }
    catch(err) {
      console.log(err);
      alert(err);
    }
  }

  // Load IP Pools
  api.pools.getAll().then(pools => {
    pools.forEach(pool => {
      $("#add-pool").append(`<option value="${pool.id}" data-value="${pool.ranges[0]}">${pool.name}</option>`);
      $("#edit-pool").append(`<option value="${pool.id}" data-value="${pool.ranges[0]}">${pool.name}</option>`);
    });
  });
  
  // Toggle dynamic inputs
  toggleDynamicInput($('#add-dns-dynamic'), $('#add-dns-server'));
  toggleDynamicInput($('#edit-dns-dynamic'), $('#edit-dns-server'));
  toggleDynamicInput($('#add-static-addressing'), $('#add-pool'));
  toggleDynamicInput($('#edit-static-addressing'), $('#edit-pool'));
  
  // FormValidation
  
  // Add server form
  $('#addServerForm').on('submit', e => {
    let data = new FormData(e.target);
    api.servers.create({
      name: data.get('Name'),
      port: data.get('Port') || null,
      mtu: data.get('MTU') || null,
      privateKey: data.get('PrivateKey'),
      ipAddress: `${data.get('IPAddress')}/${data.get('IPCidr')}`,
      useIPPool: data.get('UseIPPool') != 'on',
      ipPoolId: data.get('IPPoolId') || null,
      inheritDNS: data.get('InheritDNS') == 'on',
      dnsAddress: data.get('DNSAddress') || null,
      enabled: true
    }).then(data => {
      const toastMSG = new toastMessage("Create Server", data.body, data.title, data.background);
      const toastElement = toastMSG.getElement();
      toastContainer.append(toastElement);
      bootstrap.Toast.getOrCreateInstance(toastElement).show();
    }).catch(err => {
      console.error(err);
    }).finally(() => {
      dt_basic.ajax.reload();
    });
  });
  
  // Edit server form
  $('#editServerForm').on('submit', e => {
    let data = new FormData(e.target);
    api.servers.update(data.get('ID'), {
      name: data.get('Name') || null,
      port: data.get('Port'),
      mtu: data.get('MTU'),
      privateKey: data.get('PrivateKey') || null,
      ipAddress: `${data.get('IPAddress')}/${data.get('IPCidr')}` || null,
      useIPPool: data.get('UseIPPool') != 'on',
      ipPoolId: data.get('IPPoolId'),
      inheritDNS: data.get('InheritDNS') == 'on',
      dnsAddress: data.get('DNSAddress') || null
    }).then(data => {
      const toastMSG = new toastMessage("Update Server", data.body, data.title, data.background);
      const toastElement = toastMSG.getElement();
      toastContainer.append(toastElement);
      bootstrap.Toast.getOrCreateInstance(toastElement).show();
    }).catch(err => {
      console.error(err);
    }).finally(() => {
      dt_basic.ajax.reload();
    });
  });

  // Delete server form
  $('#removeForm').on('submit', e => {
    let data = new FormData(e.target);
    api.servers.delete(data.get('ID')).then(data => {
      const toastMSG = new toastMessage("Delete Server", data.body, data.title, data.background);
      const toastElement = toastMSG.getElement();
      toastContainer.append(toastElement);
      bootstrap.Toast.getOrCreateInstance(toastElement).show();
    }).catch(err => {
      console.error(err);
      return;
    }).finally(() => {
      dt_basic.ajax.reload();
      new bootstrap.Modal('#RemoveModal').hide();
    });
  });
});

function toggleDynamicInput(checkbox, element) {
  checkbox.on('change', function() {
    element.prop('disabled', checkbox.is(':checked'));
  });
}
