class toastMessage {
    constructor(title, message, time, background) {
      this.title = title;
      this.message = message;
      this.time = time;
      this.background = background;
    }

    getElement() {
      return $(`<div id="liveToastClass" class="toast text-bg-${this.background}" role="alert" aria-live="assertive" aria-atomic="true" data-bs-autohide="false">` +
      `  <div class="toast-header">` +
      `    <i class="bi bi-info-lg"></i>` +
      `    <strong class="me-auto">${this.title}</strong>` +
      `    <small class="text-body-secondary">${this.time}</small>` +
      `    <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>` +
      `  </div>` +
      `  <div class="toast-body">` +
      `    ${this.message}` +
      `  </div>` +
      `</div>`);
    }

    getInstance() {
      return bootstrap.Toast.getOrCreateInstance(this.getElement());
    }
}

function randomBgColor() {
  var index = Math.floor(Math.random() * 6);
  var colors = ['success', 'danger', 'warning', 'info', 'dark', 'primary', 'secondary'];
  return colors[index];
}

function convertByteSize(value, decimalPlaces = 2) {
  if (decimalPlaces < 0) {
    throw new Error("decimalPlaces must be non-negative.");
  }
  if (value < 0) {
    return `-${convertByteSize(-value, decimalPlaces)}`;
  }
  if (value === 0) {
    return "0";
  }

  const sizeSuffixes = ["bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];

  let mag = Math.floor(Math.log(value) / Math.log(1024));

  let adjustedSize = value / (1 << (mag * 10));

  if (Math.round(adjustedSize, decimalPlaces) >= 1000) {
    mag++;
    adjustedSize /= 1024;
  }

  return `${adjustedSize.toFixed(decimalPlaces)} ${sizeSuffixes[mag]}`;
}

function getIPInfo(ipAddress) {
  const url = './api/iplookup';

  const request = new XMLHttpRequest();
  request.open('GET', url, false);
  
  request.send();

  if (request.status !== 200) {
    console.error('Failed to fetch data');
    return null;
  }

  const data = JSON.parse(request.responseText);
  return data;
}
