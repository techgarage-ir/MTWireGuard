const api = new APIClient();

let renderTableCell = (function () {
    function renderTitle(title) {
        return title
            .replace('_', ' ')
            .split(" ")
            .map(word => word.charAt(0).toUpperCase() + word.slice(1))
            .join(" ");
    }

    function renderTopics(data) {
        let output = '';
        $.each(data, function (index) {
            let topic = data[index];
            switch (topic.toLowerCase()) {
                case 'system':
                    output += `<span class="badge bg-dark border me-1">${topic}</span>`;
                    break;
                case 'info':
                    output += `<span class="badge bg-info border me-1">${topic}</span>`;
                    break;
                case 'error':
                case 'critical':
                    output += `<span class="badge bg-danger border me-1">${topic}</span>`;
                    break;
                case 'account':
                    output += `<span class="badge bg-secondary border me-1">${topic}</span>`;
                    break;
                case 'dhcp':
                case 'ppp':
                case 'l2tp':
                case 'pptp':
                case 'sstp':
                default:
                    output += `<span class="badge bg-light border me-1">${topic}</span>`;
                    break;
            }
        });
        return output;
    }

    function renderDateTime(data) {
        if (!data.includes('/')) {
            return data;
        }
        const parts = data.split(' '); // Split the string into date and time parts
        const datePart = parts[0];
        const timePart = parts[1];

        // Manually map month names to their numeric values
        const monthMap = {
            jan: 0,
            feb: 1,
            mar: 2,
            apr: 3,
            may: 4,
            jun: 5,
            jul: 6,
            aug: 7,
            sep: 8,
            oct: 9,
            nov: 10,
            dec: 11
        };

        const [month, day] = datePart.split('/');

        const year = new Date().getFullYear(); // Assuming current year

        // Create a new date object with the extracted parts
        const dateObj = new Date(year, monthMap[month.toLowerCase()], day);

        // Extract the time parts
        const [hours, minutes, seconds] = timePart.split(':');

        // Set the time on the date object
        dateObj.setHours(hours);
        dateObj.setMinutes(minutes);
        dateObj.setSeconds(seconds);

        // Format the date in a standard format (e.g., "YYYY-MM-DD HH:MM:SS")
        const formattedDate = dateObj.toISOString().slice(0, 19).replace('T', ' ');

        return formattedDate;
    }

    return {
        renderTitle,
        renderTopics,
        renderDateTime
    }
})();

$(function () {
    'use strict';

    var dt_basic_table = $('.datatables-basic');

    // DataTable with buttons
    // --------------------------------------------------------------------

    if (dt_basic_table.length) {
        try {
            var dt_basic = dt_basic_table.DataTable({
                ajax: function (data, callback, settings) {
                    settings.sAjaxDataProp = '';
                    api.config.logs.getAll()
                        .then(function (response) {
                            callback(response);
                        })
                        .catch(function (error) {
                            console.log(error);
                            callback([]);
                        });
                },
                columns: [
                    { data: '' },
                    { data: 'id' },
                    { data: 'message' },
                    { data: 'topics' },
                    { data: 'time' }
                ],
                columnDefs: [
                    {
                        // For Responsive
                        targets: 0,
                        className: 'dtr-control',
                        orderable: false,
                        responsivePriority: 2,
                        searchable: false,
                        render: function (data, type, full, meta) {
                            return '';
                        }
                    },
                    {
                        // ID
                        targets: 1,
                        orderable: true,
                        searchable: false
                    },
                    {
                        // Message
                        targets: 2,
                        responsivePriority: 1
                    },
                    {
                        // Topic
                        targets: 3,
                        render: function (data, type, full, meta) {
                            return renderTableCell.renderTopics(data);
                        }
                    },
                    {
                        // Time
                        responsivePriority: 2,
                        targets: 4,
                        render: function (data, type, full, meta) {
                            return renderTableCell.renderDateTime(data);
                        }
                    }
                ],
                order: [[1, 'asc']],
                dom: '<"card-header"<"dt-action-buttons text-end"B>><"d-flex justify-content-between align-items-center row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>t<"d-flex justify-content-between row"<"col-sm-12 col-md-6"i><"col-sm-12 col-md-6"p>>',
                displayLength: 10,
                lengthMenu: [10, 25, 50, 100, 250, 500, 1000],
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
                                    exportOptions: { columns: [1, 2, 3, 4] }
                                },
                                {
                                    text: '<i class="bx bxs-file-json me-1"></i>JSON',
                                    className: 'dropdown-item',
                                    action: function (e, dt, button, config) {
                                        let array = [];
                                        let data = dt.rows().data();
                                        $.each(data, function (i) {
                                            array.push(data[i]);
                                        });

                                        $.fn.dataTable.fileSave(
                                            new Blob([JSON.stringify(array, null, 2)]),
                                            'mt-logs.json'
                                        );
                                    }
                                }
                            ]
                        }
                    ]
                },
                responsive: {
                    details: {
                        display: $.fn.dataTable.Responsive.display.modal({
                            header: function (row) {
                                var data = row.data();
                                return 'Details of ' + data['name'];
                            }
                        }),
                        type: 'column',
                        renderer: function (api, rowIdx, columns) {
                            var data = $.map(columns, function (col, i) {
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
                    "emptyTable": "No logs found"
                },
                initComplete: function (settings, json) {
                    // Start Widgets
                    const result = Object.values(json).reduce(
                        (acc, currentItem) => {
                            acc.totalItems++;
                            $.each(currentItem.topics, function (index) {
                                switch (currentItem.topics[index].toLowerCase()) {
                                    case 'critical':
                                        acc.critical++;
                                        break;
                                    case 'wireguard':
                                        acc.wireguard++;
                                        break;
                                    case 'info':
                                        acc.info++;
                                        break;
                                }
                            });
                            return acc;
                        },
                        { totalItems: 0, critical: 0, wireguard: 0, info: 0 }
                    );
                    const totalItems = result.totalItems;
                    const criticalItems = result.critical;
                    const wireguardItems = result.wireguard;
                    const infoItems = result.info;
                    $('#logs-total').text(totalItems);
                    $('#logs-critical').text(criticalItems);
                    $('#logs-wireguard').text(wireguardItems);
                    $('#logs-info').text(infoItems);
                    // END Widgets
                    // Log Filter
                    this.api()
                        .columns(3)
                        .every(function () {
                            let column = this;

                            // Create select element
                            let select = document.createElement('select');
                            select.classList.add('form-select', 'form-select-sm');
                            select.style.width = 'max-content';
                            select.add(new Option(''));
                            column.footer().replaceChildren(select);

                            // Apply listener for user change in value
                            select.addEventListener('change', function () {
                                var val = DataTable.util.escapeRegex(select.value);
                                column
                                    .search(val, true, false)
                                    .draw();
                            });

                            // Add list of options
                            let topics = new Set();

                            column
                                .data()
                                .unique()
                                .sort()
                                .toArray()
                                .flatMap(data => data.includes(',') ? data.split(',') : [data])
                                .forEach(topicArray => {
                                    topicArray.forEach(topic => {
                                        topics.add(topic);
                                    });
                                });

                            topics.forEach(topic => {
                                select.add(new Option(topic));
                            });
                        });
                    // END Log Filter
                }
            });
        }
        catch (err) {
            console.log(err);
            alert(err);
        }
    }
});

function toggleDynamicInput(checkbox, element) {
    checkbox.on('change', function () {
        element.prop('disabled', checkbox.is(':checked'));
    });
}
