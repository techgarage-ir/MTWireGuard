let api = new APIClient();

// Spinner
var spinner = function () {
    setTimeout(function () {
        if ($('#spinner').length > 0) {
            $('#spinner').removeClass('show');
        }
    }, 1);
};
spinner();

$("#loginForm").on("submit", function (event) {
    event.preventDefault();
    let isAdmin = $('#isAdmin').is(':checked');
    let username = $('#loginName').val(),
        password = $('#loginPassword').val();
    if (isAdmin) {
        api.auth.login(username, password)
            .then((status) => {
                if (!status) {
                    $('#loginAlert').show();
                }
            })
            .catch((err) => {
                console.error(err);
            });
    } else {
        fetch('/Client', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                username,
                password
            })
        }).then((response) => {
            if (response.status == 401) {
                $('#loginAlert').show();
            } else {
                window.location.href = "/Client";
            }
        }).catch((err) => {
            console.log(err);
            throw err;
        });
    }
});
