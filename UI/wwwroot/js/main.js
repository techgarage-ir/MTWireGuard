const api = new APIClient();

// Spinner
var spinner = function () {
    setTimeout(function () {
        if ($('#spinner').length > 0) {
            $('#spinner').removeClass('show');
        }
    }, 1);
};
spinner();

// Back to top button
$(window).on('scroll', function () {
    if ($(this).scrollTop() > 300) {
        $('.back-to-top').fadeIn('slow', function() {});
    } else {
        $('.back-to-top').fadeOut('slow', function() {});
    }
});
$('.back-to-top').on('click', function () {
    $("html, body").stop().animate({scrollTop:0}, 'fast', 'swing', function() {});
    return false;
});

// Sidebar Toggler
$('.sidebar-toggler').on('click', function () {
    $('.sidebar, .content').toggleClass("open");
    return false;
});

// Password Visibility
$('.password-eye').on('click', function () {
    let icon = $(this).find('i');
    let input = $(this).closest('.form-password-toggle').find('input');
    
    input.attr('type', input.attr('type') === 'password' ? 'text' : 'password');
    icon.toggleClass('bx-hide bx-show-alt');
});

// Wireguard keygen
$('.keygen-button').on('click', e => {
  let dom = $(e.target);
  let keys = wireguard.generateKeypair();
  let privateKey = keys.privateKey;
  let publicKey = keys.publicKey;
  if (dom) {
    let frm = dom.closest('form');
    frm.find('input[name="PrivateKey"]').val(privateKey);
    frm.find('input[name="PublicKey"]').val(publicKey);
  }
});
$('.key-reset-button').on('click', e => {
  let dom = $(e.target);
  if (dom) {
    let frm = dom.closest('form');
    frm.find('input[name="PrivateKey"]').val('');
    frm.find('input[name="PublicKey"]').val('');
  }
});

// Logs
api.config.logs.getAll().then(logs => {
    logs.sort((a, b) => a.id - b.id).slice(-5).forEach(log => {
        let topics = ``;
        log.topics.forEach(topic => {
            switch (topic.toLowerCase()) {
              case 'system':
                topics += `<span class="badge bg-dark border me-1">${topic}</span>`;
                break;
              case 'info':
                topics += `<span class="badge bg-info border me-1">${topic}</span>`;
                break;
              case 'error':
              case 'critical':
                topics += `<span class="badge bg-danger border me-1">${topic}</span>`;
                break;
              case 'account':
                topics += `<span class="badge bg-secondary border me-1">${topic}</span>`;
                break;
              case 'dhcp':
              case 'ppp':
              case 'l2tp':
              case 'pptp':
              case 'sstp':
              default:
                topics += `<span class="badge bg-light border me-1">${topic}</span>`;
                break;
            }
        });
        let el = `
        <li class="dropdown-item">
            <div class="d-flex flex-column">
                <div class="d-flex justify-content-between align-items-center">
                    <small class="text-medium-emphasis">#${log.id}</small>
                    <div>
                        ${topics}
                    </div>
                </div>
                <div class="d-flex font-weight-bold text-medium-emphasis mt-1">
                    <span class="text-truncate">${log.message}</span>
                </div>
                <div class="d-flex text-truncate small">
                    <small class="text-medium-emphasis mt-1">${log.time}</small>
                </div>
            </div>
        </li><hr class="dropdown-divider">`;
        $('#logs').append(el);
    });
    $('#logs').append('<a href="/Logs" class="dropdown-item text-center">See all logs</a>');
});
