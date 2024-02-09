const assetsPath = "../assets/";
const api = new APIClient();
const toastContainer = $('.toast-container');
const onlineUsers = new Set();

let renderTableCell = (function () {
  function renderTitle(title) {
    return title
      .replace('_', ' ')
      .split(" ")
      .map(word => word.charAt(0).toUpperCase() + word.slice(1))
      .join(" ");
  }

  function renderSwitch(data, disable=false) {
    let enabled = data == true;
    let active = enabled ? ' active' : '';
    let disabled = disable ? 'disabled' : '';
    return '    <button type="button" class="btn btn-sm btn-toggle item-activation' + active + '" data-bs-toggle="button" aria-pressed="' + enabled + '" autocomplete="off"' + disabled + '>' +
    '      <div class="handle"></div>' +
    '    </button>';
  }
  
  function renderName(full) {
    var $name = full['name'],
      $lastSeen = full['lastHandshake'].replace('T', ' ')
      $userId = full['id'];

    // Avatar badge
    var stateNum = Math.floor(Math.random() * 6);
    var states = ['success', 'danger', 'warning', 'info', 'dark', 'primary', 'secondary'];
    var $state = states[stateNum],
      $name = full['name'],
      $initials = ($name || '').match(/\b\w/g)?.join('').toUpperCase() || '';
    let online = 'offline';
    // online check
    if ($lastSeen.length == 8) {
      const time = new Date(`2000-01-01T${$lastSeen}`);
      const comparison = new Date(`2000-01-01T00:03:00`);
      if (time < comparison) {
        online = 'online';
        onlineUsers.add($userId);
      };
    }
    // end online check
    let $output = '<div class="position-relative badge rounded-circle h-100 w-100 d-flex justify-content-center align-items-center bg-' + $state + '">' + $initials + '</div>';

    // Creates full output for row
    var $row_output =
      '<div class="d-flex justify-content-start align-items-center">' +
      '<div class="avatar-wrapper">' +
      '<div class="avatar avatar-' + online + ' me-2">' +
      $output +
      '</div>' +
      '</div>' +
      '<div class="d-flex flex-column">' +
      '<span class="emp_name text-truncate">' +
      $name +
      '</span>' +
      '<small class="emp_post text-truncate text-muted ms-1">' +
      $lastSeen +
      '</small>' +
      '</div>' +
      '</div>';
    return $row_output;
  }

  function renderTraffic(full) {
    let used = convertByteSize(full['trafficUsed']);
    let total = full['traffic'] == 0 ? '&#8734;' : `${full['traffic']}GB`;
    let output = `${used} / ${total}`;
    return '<span class="badge text-bg-success">' + output + '</span>';
  }

  function renderExpire(data) {
    return '<span class="badge text-bg-warning text-uppercase">' + (data.toLowerCase() != 'unlimited' ? data.replace('T', ' ').slice(0, -3) : data) + '</span>';
  }

  return {
    renderTitle,
    renderName,
    renderSwitch,
    renderTraffic,
    renderExpire
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
          { data: 'interface' },
          { data: 'address'},
          { data: 'traffic' },
          { data: 'expire' },
          { data: 'isDifferent' }
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
            responsivePriority: 1,
            render: function(data, type, full, meta) {
              return renderTableCell.renderName(full);
            }
          },
          {
            // Interface
            responsivePriority: 4,
            targets: 3
          },
          {
            // Allowed Address
            responsivePriority: 3,
            targets: 4
          },
          {
            // Traffic
            targets: 5,
            render: function(data, type, full, meta) {
              return renderTableCell.renderTraffic(full);
            }
          },
          {
            // Expire
            targets: 6,
            render: function(data, type, full, meta) {
              return renderTableCell.renderExpire(data);
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
              let dlBtns = data ? '<a href="javascript:;" class="btn text-primary btn-icon item-sync" data-bs-toggle="offcanvas" data-bs-target="#offcanvasSyncUser"><i class="bx bx-sync"></i></a>' : 
                '<a href="javascript:;" class="btn text-primary btn-icon item-qr" data-bs-toggle="modal" data-bs-target="#QRModal"><i class="bx bx-qr"></i></a>' +
                '<a href="javascript:;" class="btn text-primary btn-icon item-download"><i class="bx bxs-download"></i></a>';
              return (
                '<a href="javascript:;" class="btn text-primary btn-icon item-edit" data-bs-toggle="offcanvas" data-bs-target="#offcanvasEditUser"><i class="bx bxs-edit"></i></a>' +
                dlBtns +
                '<div class="d-inline-block">' +
                '<a href="javascript:;" class="btn text-primary btn-icon dropdown-toggle hide-arrow" data-bs-toggle="dropdown"><i class="bx bx-dots-vertical-rounded"></i></a>' +
                '<ul class="dropdown-menu dropdown-menu-end">' +
                '<li><a href="javascript:;" class="dropdown-item item-details">Details</a></li>' +
                '<div class="dropdown-divider"></div>' +
                '<li><a href="javascript:;" class="dropdown-item text-danger item-delete" data-bs-toggle="modal" data-bs-target="#RemoveModal">Delete</a></li>' +
                '</ul>' +
                '</div>'
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
                  className: 'dropdown-item'
                },
                {
                  text: '<i class="bx bxs-file-json me-1"></i>JSON',
                  className: 'dropdown-item',
                  action: function ( e, dt, button, config ) {
                    //let data = dt.buttons.exportData();
                    let array = [];
                    let data = dt.rows().data();
                    $.each(data, function(i) {
                      array.push(data[i]);
                    });

                    $.fn.dataTable.fileSave(
                        new Blob([JSON.stringify(array, null, 2)]),
                        'wg-peers.json'
                    );
                  }
                }/*,
                {
                  text: '<i class="bx bx-hdd me-1"></i>Backup',
                  className: 'dropdown-item'
                }*/
              ]
            },
            {
              text: '<i class="bx bx-plus me-1"></i> <span class="d-none d-lg-inline-block">Add New User</span>',
              className: 'btn-success create-new',
              attr: {
                "data-bs-toggle": "offcanvas",
                "data-bs-target": "#offcanvasAddUser"
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
          "emptyTable": "No users found"
        },
        initComplete: function(settings, json) {
          const result = Object.values(json).reduce(
            (acc, currentItem) => {
              acc.totalItems++;
              if (currentItem.isEnabled) {
                acc.enabledItems++;
              }
              return acc;
            },
            { totalItems: 0, enabledItems: 0 }
          );
          const totalItems = result.totalItems;
          const enabledItems = result.enabledItems;
          $('#users-total').text(totalItems);
          $('#users-active').text(enabledItems);
          $('#users-online').text(onlineUsers.size);

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
                    case 'name':
                      tdValue = renderTableCell.renderName(rowData);
                      break;
                    case 'traffic':
                      tdValue = renderTableCell.renderTraffic(rowData);
                      break;
                    case 'expire':
                      tdValue = renderTableCell.renderExpire(value);
                      break;
                    default:
                      tdValue = value;
                      break;
                  }
                  return '<tr>' +
                    '<th>' +
                    renderTableCell.renderTitle(key) +
                    '</th> ' +
                    '<td><div class="text-truncate" style="width: 35ch;">' +
                      tdValue +
                    '</div></td>' +
                  '</tr>'
                }
              else {
                return '';
              }
            }).join('');

            
            let body = data ? $('<table class="table m-0"/><tbody />').append(data) : false;
            $('#DetailsModal .modal-body').html(body);
            $('#DetailsModal .modal-title').html('Details of ' + row.data()["name"].toUpperCase());
            const detailsModal = new bootstrap.Modal('#DetailsModal');
            detailsModal.show();
          });
          dt_basic.on('click', 'a.item-qr', function (e) {
            let tr = e.target.closest('tr');
            let row = dt_basic.row(tr);
            let rowData = row.data();
            let id = rowData['id'];
            let username = rowData['name'];
            $('#QRModalLabel').text(username + "'s QR");
            api.users.qr(id).then((qrData) => {
              console.log(qrData);
              $('#qr-src').attr('src', `data:image/png;base64,${qrData}`);
            });
          });
          dt_basic.on('click', 'a.item-download', function (e) {
            let tr = e.target.closest('tr');
            let row = dt_basic.row(tr);
            let rowData = row.data();
            let id = rowData['id'];
            console.log(`DL: ${id}`);
            api.users.download(id).then((dlLink) => {
              console.log(dlLink);
                const a = document.createElement('a');
                a.href = dlLink;
                a.click();
            });
          });
          dt_basic.on('click', 'a.item-edit', function (e) {
            let tr = e.target.closest('tr');
            let row = dt_basic.row(tr)
            let rowData = row.data();
            
            $.map(rowData, function(value, key) {
              key = key.replace('_', '-');
              let field = $('#offcanvasEditUser #edit-' + key);
              if (field) {
                switch (key) {
                  case 'name':
                    $('#edit-username').val(value);
                  break;
                  case 'address':
                    $('#edit-allowed-address').val(value.replace('/32', ''));
                  break;
                  case 'publicKey':
                  case 'privateKey':
                  case 'presharedKey':
                    field.val('');
                  break;
                  case 'expire':
                    if (value != 'Unlimited')
                      $("#edit-expire").flatpickr({
                        enableTime: true,
                        altInput: true,
                        altFormat: "Y-m-d H:i",
                        dateFormat: "Z",
                        defaultDate: value.replace('T', ' ').slice(0, -3)
                      });
                    else {
                      $("#edit-expire").flatpickr({
                        enableTime: true,
                        altInput: true,
                        altFormat: "Y-m-d H:i",
                        dateFormat: "Z"
                      }).clear();
                    }
                  break;
                  case 'traffic':
                    if (value == 0)
                      field.val(value);
                    else
                      field.val(value);
                  break;
                  default:
                    field.val(value);
                  break;
                }
              }
            });
          });
          dt_basic.on('click', 'a.item-sync', function (e) {
            let tr = e.target.closest('tr');
            let row = dt_basic.row(tr)
            let rowData = row.data();
            
            $.map(rowData, function(value, key) {
              key = key.replace('_', '-');
              let field = $('#offcanvasSyncUser #sync-' + key);
              if (field) {
                switch (key) {
                  case 'name':
                    $('#sync-username').val(value);
                  break;
                  case 'publicKey':
                    $('#sync-private-key').val(value);
                  break;
                  case 'privateKey':
                    $('#sync-public-key').val(value);
                  break;
                  default:
                    console.log(`${key} => ${value}`);
                    field.val(value);
                  break;
                }
              }
            });
          });
          dt_basic.on('click', 'a.item-delete', function (e) {
            let tr = e.target.closest('tr');
            let row = dt_basic.row(tr)
            let rowData = row.data();
            let userId = rowData['id'];
            $('#removeForm input[name="ID"]').val(userId);
          });
          dt_basic.on('click', 'button.item-activation', function (e) {
            let tr = e.target.closest('tr');
            let row = dt_basic.row(tr)
            let rowData = row.data();
            let userId = rowData['id'];
            let enabled = rowData['isEnabled'];
            console.log(`${userId}: ${enabled}`);
            api.users.activate(userId, {enabled: enabled}).then(data => {
              const toastMSG = new toastMessage("Activate User", data.body, data.title, data.background);
              const toastElement = toastMSG.getElement();
              toastContainer.append(toastElement);
              bootstrap.Toast.getOrCreateInstance(toastElement).show();
            }).catch(err => {
              console.log(err);
            }).finally(() => {
              dt_basic.ajax.reload();
            });
          });
        }
      });
    }
    catch(err) {
      console.error(err);
      alert(err);
    }
  }

  // Load interface options
  api.servers.getAll().then((servers) => {
    $.each(servers, function(index) {
      let server = servers[index];
      $('#add-interface').append('<option value="' + server.name + '">' + server.name + '</option>');
      $('#edit-interface').append('<option value="' + server.name + '">' + server.name + '</option>');
    });
  });
  
  // Toggle dynamic inputs
  toggleDynamicInput($('#add-dns-dynamic'), $('#add-dns-server'));
  toggleDynamicInput($('#edit-dns-dynamic'), $('#edit-dns-address'));
  toggleDynamicInput($('#add-ip-dynamic'), $('#add-allowed-address'));
  toggleDynamicInput($('#edit-ip-dynamic'), $('#edit-allowed-address'));
  
  // FormValidation

  // Add user form field types
  $('#add-expire').flatpickr({
    enableTime: true,
    altInput: true,
    altFormat: "Y-m-d H:i",
    dateFormat: "Z"
  });
  
  // Add user form
  $('#addUserForm').on('submit', e => {
    let form = $(e.target);
    let data = new FormData(e.target);
    api.users.create({
      name: data.get('Username'),
      password: data.get('Password'),
      interface: data.get('Interface'),
      privateKey: data.get('PrivateKey'),
      publicKey: data.get('PublicKey'),
      presharedKey: data.get('PresharedKey'),
      inheritIP: data.get('InheritIP') == 'on',
      allowedAddress: data.get('AllowedAddress'),
      endpointAddress: data.get('Endpoint'),
      endpointPort: data.get('EndpointPort') || null,
      keepalive: data.get('KeepAlive') || null,
      inheritDNS: data.get('InheritDNS') == 'on',
      dnsAddress: data.get('DNSAddress'),
      expire: data.get('Expire'),
      traffic: data.get('Traffic'),
      enabled: form.find('button.btn-toggle').attr('aria-pressed') == 'true'
    }).then(data => {
      const toastMSG = new toastMessage("Create User", data.body, data.title, data.background);
      const toastElement = toastMSG.getElement();
      toastContainer.append(toastElement);
      bootstrap.Toast.getOrCreateInstance(toastElement).show();
    }).catch(err => {
      console.error(err);
    }).finally(() => {
      dt_basic.ajax.reload();
    });
  });
  
  // Edit user form
  $('#editUserForm').on('submit', e => {
    let form = $(e.target);
    let data = new FormData(e.target);
    api.users.update(data.get('ID'), {
      name: data.get('Username') || null,
      password: data.get('Password') || null,
      interface: data.get('Interface'),
      privateKey: data.get('PrivateKey') || null,
      publicKey: data.get('PublicKey') || null,
      presharedKey: data.get('PresharedKey') || null,
      inheritIP: data.get('InheritIP') == 'on',
      allowedAddress: data.get('AllowedAddress') || null,
      endpointAddress: data.get('Endpoint') || null,
      endpointPort: data.get('EndpointPort') || null,
      keepalive: data.get('KeepAlive') || null,
      inheritDNS: data.get('InheritDNS') == 'on',
      dnsAddress: data.get('DNSAddress') || null,
      expire: data.get('Expire') || null,
      traffic: data.get('Traffic') || null,
      enabled: form.find('button.btn-toggle').attr('aria-pressed') == 'true'
    }).then(data => {
      const toastMSG = new toastMessage("Update User", data.body, data.title, data.background);
      const toastElement = toastMSG.getElement();
      toastContainer.append(toastElement);
      bootstrap.Toast.getOrCreateInstance(toastElement).show();
    }).catch(err => {
      console.error(err);
    }).finally(() => {
      dt_basic.ajax.reload();
    });
  });
  
  // Sync user form
  $('#syncUserForm').on('submit', e => {
    let data = new FormData(e.target);
    api.users.sync(data.get('ID'), {
      name: data.get('Username') || null,
      password: data.get('Password') || null,
      privateKey: data.get('PrivateKey') || null,
      publicKey: data.get('PublicKey') || null
    }).then(data => {
      const toastMSG = new toastMessage("Sync User", data.body, data.title, data.background);
      const toastElement = toastMSG.getElement();
      toastContainer.append(toastElement);
      bootstrap.Toast.getOrCreateInstance(toastElement).show();
    }).catch(err => {
      console.error(err);
    }).finally(() => {
      dt_basic.ajax.reload();
    });
  });

  // Delete user form
  $('#removeForm').on('submit', e => {
    let data = new FormData(e.target);
    api.users.delete(data.get('ID')).then(data => {
      const toastMSG = new toastMessage("Delete User", data.body, data.title, data.background);
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

