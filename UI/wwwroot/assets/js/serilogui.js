let favicon = document.createElement('link');
favicon.href = 'img/favicon.ico';
favicon.rel = 'icon';
document.getElementsByTagName('head')[0].appendChild(favicon);
document.querySelector('#sidebar.active .logo:first-child').innerHTML = 'MW';
document.querySelector('#sidebar.active .logo:last-child').innerHTML = 'MTWireguard';
