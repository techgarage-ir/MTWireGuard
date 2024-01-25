const assetsPath = "../assets/";
const api = new APIClient();
const toastContainer = $('.toast-container');

let renderTableCell = (function () {
  function renderTitle(title) {
    return title.replace('_', ' ').replace(/(\b\w)/g, char => char.toUpperCase());
  }

  function renderBadge(data) {
    return '<span class="badge text-bg-warning text-uppercase">' + data + '</span>';
  }

  function renderRanges(data, isDetails = false) {
    let detailsClass = isDetails ? ' class="d-flex flex-column"' : '';
    let output = `<div${detailsClass}>`;
    let margin = isDetails ? 'mb-2' : 'me-2';
    let spanStyle = isDetails ? ' style="width: max-content;"' : '';
    $.map(data, range => {
      output += `<span class="badge text-bg-${randomBgColor()} text-uppercase ${margin}"${spanStyle}>${range}</span>`;
    });
    return output + '</div>';
  }

  return {
    renderTitle,
    renderBadge,
    renderRanges
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
          api.pools.getAll()
            .then(function(response) {
              callback(response);
            })
            .catch(function(error) {
              console.log(error);
              callback([]);
            });
        },
        columns: [
          { data: '' },
          { data: 'name' },
          { data: 'ranges' },
          { data: 'nextPool' },
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
            // Name
            targets: 1,
            responsivePriority: 1
          },
          {
            // Ranges
            responsivePriority: 3,
            targets: 2,
            render: function(data, type, full, meta) {
              return renderTableCell.renderRanges(data);
            }
          },
          {
            // Next Pool
            targets: 3,
            responsivePriority: 4
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
                '<a href="javascript:;" class="btn text-primary btn-icon item-edit" data-bs-toggle="offcanvas" data-bs-target="#offcanvasEditPool"><i class="bx bxs-edit"></i></a>' +
                '<a href="javascript:;" class="btn text-primary btn-icon item-details"><i class="bx bx-detail"></i></a>' +
                '<a href="javascript:;" class="btn text-danger btn-icon item-delete"><i class="bx bx-trash"></i></a>'
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
              text: '<i class="bx bx-plus me-1"></i> <span class="d-none d-lg-inline-block">Add New Pool</span>',
              className: 'btn-success rounded-3 create-new',
              attr: {
                "data-bs-toggle": "offcanvas",
                "data-bs-target": "#offcanvasAddPool"
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
          "emptyTable": "No IP pools found"
        },
        initComplete: function(settings, json) {
          dt_basic.on('click', 'a.item-details', function (e) {
            let tr = e.target.closest('tr');
            let row = dt_basic.row(tr)
            let rowData = row.data();
            let data = `<tr><th>ID</th><td>${rowData['id']}</td></tr>` +
              `<tr><th>Name</th><td>${rowData['name']}</td></tr>` +
              `<tr><th>Ranges</th><td>${renderTableCell.renderRanges(rowData['ranges'], true)}</td></tr>`;
            if (rowData['nextPool'])
              data += `<tr><th>Next Pool</th><td>${rowData['nextPool']}</td></tr>`;
            
            let body = data ? $('<table class="table"/><tbody />').append(data) : false;
            console.log(data);
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
              let field = $('#offcanvasEditPool #edit-' + key);
              if (field) {
                switch (key) {
                  case 'ranges':
                    let count = 1;
                    $('#edit-addresses').empty();
                    value.forEach(range => {
                      var inputField = '<div class="input-group mb-1">' +
                                          `<input type="text" name="Ranges[]" class="form-control" aria-label="Range" aria-describedby="removeField${count}" value="${range}">` +
                                          `<span class="input-group-text text-danger" id="removeField${count}" role="button" onclick="removeRangeInputGroup(this)">` +
                                          '<i class="bx bx-minus"></i>' +
                                          '</span>' +
                                      '</div>';
                      $('#edit-addresses').append(inputField);
                      count++;
                    });
                    break;
                  default:
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
            let poolId = rowData['id'];
            api.pools.delete(poolId).then(data => {
              const toastMSG = new toastMessage("Delete IP Pool", data.body, data.title, data.background);
              const toastElement = toastMSG.getElement();
              toastContainer.append(toastElement);
              bootstrap.Toast.getOrCreateInstance(toastElement).show();
            }).catch(err => {
              console.log(err);
              return;
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
  
  // Add pool form
  $('#addPoolForm').on('submit', e => {
    let data = new FormData(e.target);
    api.pools.create({
      name: data.get('Name'),
      ranges: data.getAll('Ranges[]'),
      next: data.get('NextPool') || null
    }).then(data => {
      const toastMSG = new toastMessage("Create IP Pool", data.body, data.title, data.background);
      const toastElement = toastMSG.getElement();
      toastContainer.append(toastElement);
      bootstrap.Toast.getOrCreateInstance(toastElement).show();
    }).catch(err => {
      console.log(err);
    }).finally(() => {
      dt_basic.ajax.reload();
    });
  });
  
  // Edit pool form
  $('#editPoolForm').on('submit', e => {
    let data = new FormData(e.target);
    api.pools.update(data.get('ID'), {
      name: data.get('Name'),
      ranges: data.getAll('Ranges[]'),
      next: data.get('NextPool')
    }).then(data => {
      const toastMSG = new toastMessage("Update IP Pool", data.body, data.title, data.background);
      const toastElement = toastMSG.getElement();
      toastContainer.append(toastElement);
      bootstrap.Toast.getOrCreateInstance(toastElement).show();
    }).catch(err => {
      console.log(err);
    }).finally(() => {
      dt_basic.ajax.reload();
    });
  });

  // Load next pool options
  api.pools.getAll().then((pools) => {
    $.each(pools, function(index) {
      let pool = pools[index];
      $('#add-next').append('<option value="' + pool.name + '">' + pool.name + '</option>');
      $('#edit-next').append('<option value="' + pool.name + '">' + pool.name + '</option>');
    });
  });
  
  // FormValidation
});

// Add range input field
$('#addRangeBtn').on('click', function(e) {
  addRangeInputField('add');
});
$('#editRangeBtn').on('click', function(e) {
  addRangeInputField('edit');
});

// Remove range input field
function removeRangeInputGroup(e) {
  $(e).closest('.input-group').remove();
}

// Add range input field
function addRangeInputField(section) {
  var count = $(`#${section}-addresses input`).length + 1;
  var inputField = '<div class="input-group mb-1">' +
                      `<input type="text" name="Ranges[]" class="form-control" aria-label="Range" aria-describedby="remove${section}Field${count}">` +
                      `<span class="input-group-text text-danger" id="remove${section}Field${count}" role="button" onclick="removeRangeInputGroup(this)">` +
                      '<i class="bx bx-minus"></i>' +
                      '</span>' +
                  '</div>';
  $(`#${section}-addresses`).append(inputField);
}
