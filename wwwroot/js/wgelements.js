const SVG_XLINK = "http://www.w3.org/1999/xlink";

function generateKeys(dom) {
    let keys = wireguard.generateKeypair();
    let privateKey = keys.privateKey;
    let publicKey = keys.publicKey;
    if (dom) {
        let frm = dom.closest('form');
        frm.querySelector('input[id$="PrivKey"]').value = privateKey;
        frm.querySelector('input[id$="PubKey"]').value = publicKey;
    }
}

function changeIcon(btn) {
    let frm = btn.closest('form');
    let pk = frm.querySelector('input[id$="PubKey"]');
    pk.toggleAttribute('disabled');
    let svg = btn.children.item(0);
    let use = svg.children.item(0);
    let icon = use.getAttribute('xlink:href');
    if (icon.endsWith('unlocked')) {
        use.setAttributeNS(SVG_XLINK, 'xlink:href', 'vendors/coreui/icons/svg/free.svg#cil-lock-locked');
    } else {
        use.setAttributeNS(SVG_XLINK, 'xlink:href', 'vendors/coreui/icons/svg/free.svg#cil-lock-unlocked');
    }
}
