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
    btn.addEventListener('click', ddBtnClick);
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

function ddBtnClick(event) {
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
