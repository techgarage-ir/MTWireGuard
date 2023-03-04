let container = document.getElementById('toastContainer');
let Forms = document.querySelectorAll('form');
let dropdownBTNs = document.querySelectorAll('a[data-target="_self"]');

Forms.forEach(form => {
    form.addEventListener('submit', function (event) {
        event.preventDefault();
        let data = this;
        let formData = new FormData(data);
        fetch(data.getAttribute('action'), {
            method: data.getAttribute('method'),
            body: formData
        }).then(res => res.json())
            .then(function (data) {
                var toast = document.createElement('div');
                toast.className = 'toast text-bg-' + data.background;
                toast.setAttribute('role', 'alert');
                toast.setAttribute('aria-live', 'assertive');
                toast.setAttribute('aria-atomic', 'true');
                toast.innerHTML = `<div class='toast-header'>
                        <strong class='me-auto'>${data['title']}</strong>
                        <button type='button' class='btn-close' data-coreui-dismiss='toast' aria-label='Close'></button>
                    </div>
                    <div class='toast-body'>${data['body']}</div>`;
                toast.addEventListener('hidden.coreui.toast', (event) => {
                    event.target.remove();
                });
                container.appendChild(toast);
                let toastAlert = new coreui.Toast(toast, { "delay": 5000, "autohide": true });
                toastAlert.show();
                refreshTables();
            });
    });
});

dropdownBTNs.forEach(btn => {
    btn.addEventListener('click', dropdownBtnClick);
});


document.addEventListener("DOMContentLoaded", function (event) {
    refreshTables();
});

function loadServers() {
    fetch("/Servers/getAll")
        .then((response) => response.text())
        .then((data) => {
            document.querySelector('table tbody').innerHTML = data;
        });
}
function loadClients() {
    fetch("/Clients/getAll")
        .then((response) => response.text())
        .then((data) => {
            document.querySelector('table tbody').innerHTML = data;
        });
}

function refreshTables() {
    if (window.location.href.endsWith("Clients")) {
        loadClients();
    } else if (window.location.href.endsWith("Servers")) {
        loadServers();
    }
}

function dropdownBtnClick(event) {
    event.preventDefault();
    fetch(event.target.getAttribute('href'))
        .then(res => res.json())
        .then(function (data) {
            var toast = document.createElement('div');
            toast.className = 'toast text-bg-' + data.background;
            toast.setAttribute('role', 'alert');
            toast.setAttribute('aria-live', 'assertive');
            toast.setAttribute('aria-atomic', 'true');
            toast.innerHTML = `<div class='toast-header'>
                        <strong class='me-auto'>${data['title']}</strong>
                        <button type='button' class='btn-close' data-coreui-dismiss='toast' aria-label='Close'></button>
                    </div>
                    <div class='toast-body'>${data["body"]}</div>`;
            toast.addEventListener('hidden.coreui.toast', (event) => {
                event.target.remove();
            });
            container.appendChild(toast);
            let toastAlert = new coreui.Toast(toast, { "delay": 5000, "autohide": true });
            toastAlert.show();
            refreshTables();
        });
}

function deleteBtn(event) {
    let Id = event.target.closest('tr').getAttribute('data-id');
    document.querySelector('#DeleteModal input[name="Id"]').value = Id;
}

function syncBtn(event) {
    let Id = event.target.closest('tr').getAttribute('data-id');
    document.querySelector('#SyncModal input[name="ID"]').value = Id;
}

function updateClientBtn(event) {
    document.querySelector('#EditModal form').reset();
    let row = event.target.closest('tr');
    let Id = row.getAttribute('data-id');
    let name = row.querySelector('td:nth-child(2)').innerText;
    let interface = row.querySelector('td:nth-child(3)').innerText;
    let address = row.querySelector('td:nth-child(4)').innerText;
    let publicKey = row.querySelector('td:nth-child(5)').innerText;
    document.querySelector('#EditModal input[name="ID"]').value = Id;
    document.querySelector('#EditModal input[name="Name"]').placeholder = name;
    document.querySelector('#EditModal input[name="AllowedAddress"]').placeholder = address;
    document.querySelector('#EditModal input[name="PublicKey"]').placeholder = publicKey;
    let ifOption = document.querySelector('#EditModal option[value="' + interface + '"]');
    if (ifOption)
        ifOption.setAttribute("selected", true);
}

function updateServerBtn(event) {
    document.querySelector('#EditModal form').reset();
    let row = event.target.closest('tr');
    let Id = row.getAttribute('data-id');
    let name = row.querySelector('td:nth-child(2)').innerText;
    let port = row.querySelector('td:nth-child(3)').innerText;
    let mtu = row.querySelector('td:nth-child(4)').innerText;
    let publicKey = row.querySelector('td:nth-child(5)').innerText;
    document.querySelector('#EditModal input[name="ID"]').value = Id;
    document.querySelector('#EditModal input[name="Name"]').placeholder = name;
    document.querySelector('#EditModal input[name="Port"]').placeholder = port;
    document.querySelector('#EditModal input[name="MTU"]').placeholder = mtu;
    document.querySelector('#EditModal input[id$="PubKey"]').placeholder = publicKey;
}
