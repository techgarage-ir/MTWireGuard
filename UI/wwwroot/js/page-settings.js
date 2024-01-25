const api = new APIClient();
const toastContainer = $('.toast-container');

(function ($) {
    "use strict";
    // Add server input field
    $('#addServerBtn').on('click', function(e) {
        addServerInputField();
    });

    // DNS
    loadDNSBox();

    // Identity
    api.config.identity.get().then(data => {
        $('#identityName').val(data["name"]);
    });

    // Update Resource Values
    api.config.resources.get().then(function (data) {
        let
            freeRAM = data["ram"]["free"].replace('.00', ''),
            usedRAM = data["ram"]["used"].replace('.00', ''),
            totalRAM = data["ram"]["total"].replace('.00', ''),
            percRAM = data["ram"]["percentage"],
            freeHDD = data["hdd"]["free"].replace('.00', ''),
            usedHDD = data["hdd"]["used"].replace('.00', ''),
            totalHDD = data["hdd"]["total"].replace('.00', ''),
            percHDD = data["hdd"]["percentage"],
            cpu = data["cpuLoad"],
            uptime = data["upTime"];
        
        // ProgressBar colors
        let hddColor = "bg-info";
        if (percHDD <= 25) hddColor = "bg-info";
        else if (percHDD <= 75) hddColor = "bg-warning";
        else hddColor = "bg-danger";

        let ramColor = "bg-info";
        if (percRAM <= 25) ramColor = "bg-info";
        else if (percRAM <= 75) ramColor = "bg-warning";
        else ramColor = "bg-danger";

        // RAM Box
        $('#ram-box').find('.fs-4').text(freeRAM);
        $('#ram-box').find('.progress-bar').addClass(ramColor);
        $('#ram-box').find('.progress-bar').width(`${percRAM}%`);
        $('#ram-box').find('.progress-bar').attr('aria-valuenow', percRAM);
        $('#ram-box').find('.text-medium-emphasis-inverse').text(`Used ${usedRAM} of ${totalRAM}`);
        // HDD Box
        $('#hdd-box').find('.fs-4').text(freeHDD);
        $('#hdd-box').find('.progress-bar').addClass(hddColor);
        $('#hdd-box').find('.progress-bar').width(`${percHDD}%`);
        $('#hdd-box').find('.progress-bar').attr('aria-valuenow', percHDD);
        $('#hdd-box').find('.text-medium-emphasis-inverse').text(`Used ${usedHDD} of ${totalHDD}`);
        // UPTime Box
        $('#uptime-box').find('.fs-4').text(uptime);
        $('#uptime-box').find('.fs-5.fw-lighter').text(`CPU: ${cpu}%`);
    });

    // Identity form
    $('#identityForm').on('submit', e => {
        let data = new FormData(e.target);
        api.config.identity.update({
            name: data.get('Name')
        }).then(data => {
            const toastMSG = new toastMessage("Update System Identity", data.body, data.title, data.background);
            const toastElement = toastMSG.getElement();
            toastContainer.append(toastElement);
            bootstrap.Toast.getOrCreateInstance(toastElement).show();
        }).catch(err => {
            console.log(err);
        }).finally(() => {
            api.config.identity.get().then(data => {
                $('#identityName').val(data["name"]);
            });
        });
    });

    // DNS form
    $('#dnsForm').on('submit', e => {
        let data = new FormData(e.target);
        api.config.dns.update({
            servers: data.getAll('servers[]')
        }).then(data => {
            const toastMSG = new toastMessage("Update System DNS", data.body, data.title, data.background);
            const toastElement = toastMSG.getElement();
            toastContainer.append(toastElement);
            bootstrap.Toast.getOrCreateInstance(toastElement).show();
        }).catch(err => {
            console.log(err);
        }).finally(() => {
            loadDNSBox();
        });
    });
})(jQuery);

// Remove server input field
function removeServerInputGroup(e) {
    $(e).closest('.input-group').remove();
}

// Add server input field
function addServerInputField(serverAddress = "") {
    var count = $('#serversGroup input').length + 1;
    let value = ` value="${serverAddress}"` ?? "";
    var inputField = '<div class="input-group mb-1">' +
                        `<input type="text" name="servers[]" class="form-control" ${value} placeholder="Server ${count}" aria-label="DNS Server" aria-describedby="removeField${count}">` +
                        `<span class="input-group-text text-danger" id="removeField${count}" role="button" onclick="removeServerInputGroup(this)">` +
                        '<i class="bx bx-minus"></i>' +
                        '</span>' +
                    '</div>';
    $('#serversGroup').append(inputField);
}

// Add dynamic server
function addDynServerField(serverAddress) {
    let field = `<span class="d-block my-2 p-2 rounded-1 border border-1">${serverAddress}</span>`;
    $('#dynamicServersGroup').append(field);
}

// Load DNS
function loadDNSBox() {
    $('#serversGroup').html('');
    $('#dynamicServersGroup').html('');
    api.config.dns.get().then(function (data) {
        let servers = data["servers"].split(',');
        let dynamicServers = data["dynamicServers"].split(',');

        servers.forEach(server => {
            addServerInputField(server);
        });
        dynamicServers.forEach(server => {
            addDynServerField(server);
        });
    });
}