// ── Şifre göster/gizle (kapanmış satırlar) ───────────────────────────────
function toggleEditSecret(spanId, btn, value) {
    var span = document.getElementById(spanId);
    var icon = btn.querySelector('i');
    if (!window._secretState) window._secretState = {};
    if (window._secretState[spanId]) {
        span.textContent = '••••••';
        icon.className = 'mdi mdi-eye';
        icon.style.color = '#888';
        window._secretState[spanId] = false;
    } else {
        span.textContent = value;
        icon.className = 'mdi mdi-eye-off';
        icon.style.color = '#e84545';
        window._secretState[spanId] = true;
    }
}

// ── Toggle edit password ──────────────────────────────────────────────────
function toggleEditPw(inputId, btn) {
    var input = document.getElementById(inputId);
    var icon = btn.querySelector('i');
    if (input.type === 'password') {
        input.type = 'text';
        icon.className = 'mdi mdi-eye-off';
    } else {
        input.type = 'password';
        icon.className = 'mdi mdi-eye';
    }
}

// ── Sync hidden input ─────────────────────────────────────────────────────
function syncHidden(hiddenId, value) {
    var hidden = document.getElementById(hiddenId);
    if (hidden) hidden.value = value;
}

// ── fixDecimal ────────────────────────────────────────────────────────────
function fixDecimal(input) {
    input.value = input.value.replace(',', '.');
    input.value = input.value.replace(/[^0-9.]/g, '');
}

// ── Satır sil ─────────────────────────────────────────────────────────────
function removeEditRow(idx) {
    Swal.fire({
        title: 'Satırı Sil',
        text: 'Bu satırı silmek istediğinize emin misiniz?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Evet, Sil',
        cancelButtonText: 'İptal',
        confirmButtonColor: '#e84545',
        cancelButtonColor: '#444',
        background: '#1a1a2e',
        color: '#fff'
    }).then(function (r) {
        if (!r.isConfirmed) return;

        var row = document.getElementById('editRow_' + idx);
        if (row) row.remove();

        document.querySelectorAll('[name^="Items[' + idx + ']"]')
            .forEach(function (el) { el.remove(); });

        var rightPanel = document.getElementById('rightPanel_' + idx);
        if (rightPanel) rightPanel.remove();

        var count = document.querySelectorAll('#itemsTableBody tr').length;
        document.getElementById('rowCount').textContent = count;

        if (count === 0) {
            document.getElementById('itemsTableBody').innerHTML =
                '<tr id="emptyRow"><td colspan="7" class="text-center text-muted py-3">' +
                '<i class="mdi mdi-information-outline mr-1"></i>' +
                'Henüz satır eklenmedi.</td></tr>';
        }
    });
}

// ── Modal: Fiyat doldur ───────────────────────────────────────────────────
function modalFillPrice() {
    var sel = document.getElementById('m_ProductId');
    var opt = sel.options[sel.selectedIndex];
    if (opt && opt.dataset.price) {
        var priceInput = document.getElementById('m_UnitPrice');
        priceInput.value = parseFloat(opt.dataset.price).toFixed(2);
        priceInput.style.borderColor = '#28a745';
        setTimeout(function () { priceInput.style.borderColor = ''; }, 1500);
    }
    document.getElementById('stockInfo').textContent =
        opt && opt.dataset.stock ? 'Mevcut stok: ' + opt.dataset.stock : '';
    modalCheckStock();
}

function modalCheckStock() {
    var sel = document.getElementById('m_ProductId');
    var opt = sel.options[sel.selectedIndex];
    var qty = parseInt(document.getElementById('m_Quantity').value) || 0;
    var stock = parseInt(opt.dataset.stock) || 0;
    var warn = document.getElementById('stockWarning');
    if (opt.value && qty > stock) {
        warn.style.display = 'block';
        warn.textContent = 'Yetersiz stok! Mevcut: ' + stock;
    } else {
        warn.style.display = 'none';
    }
}

// ── Göster / Gizle ────────────────────────────────────────────────────────
function togglePw(inputId, btn) {
    var input = document.getElementById(inputId);
    var icon = btn.querySelector('i');
    if (input.type === 'password') {
        input.type = 'text';
        icon.className = 'mdi mdi-eye-off';
    } else {
        input.type = 'password';
        icon.className = 'mdi mdi-eye';
    }
}

// ── Mail validasyon ───────────────────────────────────────────────────────
function validateCharMail(input) {
    var val = input.value.trim();
    var errorEl = document.getElementById('charMailError');
    if (val.length === 0) {
        errorEl.style.display = 'none';
        input.style.borderColor = '';
        return;
    }
    var pattern = /^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$/;
    if (!pattern.test(val)) {
        errorEl.textContent = 'Geçerli bir email formatı giriniz.';
        errorEl.style.display = 'block';
        input.style.borderColor = '#e84545';
    } else {
        errorEl.style.display = 'none';
        input.style.borderColor = '#28a745';
    }
}

// ── ZIP seç ───────────────────────────────────────────────────────────────
function handleZipSelect(input) {
    var label = input.nextElementSibling;
    var preview = document.getElementById('zipPreview');
    var progress = document.getElementById('zipProgress');
    currentZipBase64 = null;
    currentZipName = null;

    if (!input.files || input.files.length === 0) return;
    var file = input.files[0];

    if (!file.name.toLowerCase().endsWith('.zip')) {
        Swal.fire({
            title: 'Geçersiz Dosya!', text: 'Sadece .zip yükleyebilirsiniz.',
            icon: 'error', confirmButtonColor: '#e84545', background: '#1a1a2e', color: '#fff'
        });
        input.value = '';
        label.textContent = 'Dosya seç...';
        return;
    }
    if (file.size > 10 * 1024 * 1024) {
        Swal.fire({
            title: 'Çok Büyük!', text: 'Max 10MB.',
            icon: 'warning', confirmButtonColor: '#e84545', background: '#1a1a2e', color: '#fff'
        });
        input.value = '';
        label.textContent = 'Dosya seç...';
        return;
    }

    label.textContent = file.name;
    progress.style.display = 'block';
    preview.innerHTML = '';

    var reader = new FileReader();
    reader.onload = function (e) {
        currentZipBase64 = e.target.result;
        currentZipName = file.name;
        progress.style.display = 'none';
        preview.innerHTML =
            '<span class="badge badge-success">' +
            '<i class="mdi mdi-check-circle mr-1"></i>' +
            escHtml(file.name) + ' \u2014 ' + (file.size / 1024).toFixed(1) + ' KB</span>';
    };
    reader.onerror = function () {
        progress.style.display = 'none';
        Swal.fire({
            title: 'Hata!', text: 'Dosya okunamadi.',
            icon: 'error', confirmButtonColor: '#e84545', background: '#1a1a2e', color: '#fff'
        });
    };
    reader.readAsDataURL(file);
}

// ── Modal'dan satır ekle ──────────────────────────────────────────────────
function addItemFromModal() {
    var sel = document.getElementById('m_ProductId');
    var opt = sel.options[sel.selectedIndex];
    var qty = parseInt(document.getElementById('m_Quantity').value) || 0;
    var stock = parseInt(opt.dataset.stock) || 0;
    var charMail = document.getElementById('m_CharacterMail').value.trim();
    var emailPattern = /^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$/;

    if (!sel.value) { showErr('Urun seciniz!'); return; }
    if (qty < 1) { showErr('Miktar en az 1!'); return; }
    if (qty > stock) { showErr('Yetersiz stok! Mevcut: ' + stock); return; }
    if (!document.getElementById('m_ServerName').value.trim()) { showErr('Server adi zorunludur!'); return; }
    if (!document.getElementById('m_CharacterId').value.trim()) { showErr('Karakter ID zorunludur!'); return; }
    if (!document.getElementById('m_CharacterPw').value.trim()) { showErr('Karakter sifresi zorunludur!'); return; }
    if (!charMail) { showErr('Karakter mail zorunludur!'); return; }
    if (!emailPattern.test(charMail)) { showErr('Gecerli mail giriniz!'); return; }
    if (!document.getElementById('m_CharacterMailPw').value.trim()) { showErr('Mail sifresi zorunludur!'); return; }
    if (!document.getElementById('m_OtpCode').value.trim()) { showErr('OTP kodu zorunludur!'); return; }
    if (!document.getElementById('m_OtpPassword').value.trim()) { showErr('OTP sifresi zorunludur!'); return; }
    if (!currentZipBase64) { showErr('ZIP dosyasi zorunludur!'); return; }

    var idx = newItemIndex;
    var serverName = document.getElementById('m_ServerName').value.trim();
    var charId = document.getElementById('m_CharacterId').value.trim();
    var charPw = document.getElementById('m_CharacterPw').value.trim();
    var mailPw = document.getElementById('m_CharacterMailPw').value.trim();
    var otpCode = document.getElementById('m_OtpCode').value.trim();
    var otpPw = document.getElementById('m_OtpPassword').value.trim();
    var unitPrice = (document.getElementById('m_UnitPrice').value || '0').replace(',', '.');

    // ── Sol tablo satırı ─────────────────────────────────────────────────
    var tbody = document.getElementById('itemsTableBody');
    var emptyRow = document.getElementById('emptyRow');
    if (emptyRow) emptyRow.remove();

    var tr = document.createElement('tr');
    tr.id = 'editRow_' + idx;
    tr.innerHTML =
        '<td>' + (tbody.children.length + 1) + '</td>' +
        '<td><strong style="color:#fff;">' + escHtml(opt.dataset.name) + '</strong><br>' +
        '<small class="text-muted">' + escHtml(opt.dataset.code) + '</small></td>' +
        '<td style="color:#ccc;">' + escHtml(serverName) + '</td>' +
        '<td>' +
        '<input type="number" name="Items[' + idx + '].Quantity"' +
        ' class="form-control form-control-sm" value="' + qty + '" min="1"' +
        ' style="width:80px;" />' +
        '</td>' +
        '<td>' +
        '<input type="text" name="Items[' + idx + '].UnitPriceRaw"' +
        ' class="form-control form-control-sm" value="' + unitPrice + '"' +
        ' style="width:100px;" oninput="fixDecimal(this)" />' +
        '</td>' +
        '<td><span class="badge badge-success">' +
        '<i class="mdi mdi-check-circle mr-1"></i>ZIP Var</span></td>' +
        '<td><button type="button" class="btn btn-danger btn-sm"' +
        ' onclick="removeEditRow(' + idx + ')">' +
        '<i class="mdi mdi-delete"></i></button></td>';
    tbody.appendChild(tr);

    // ── Hidden inputlar ──────────────────────────────────────────────────
    var newInputs = document.getElementById('newItemInputs');

    function addHidden(name, value, id) {
        var input = document.createElement('input');
        input.type = 'hidden';
        input.name = name;
        input.value = value || '';
        if (id) input.id = id;
        newInputs.appendChild(input);
        return input;
    }

    addHidden('Items[' + idx + '].Id', '');
    addHidden('Items[' + idx + '].ProductId', sel.value);
    addHidden('Items[' + idx + '].ZipFile', currentZipBase64);
    addHidden('Items[' + idx + '].ZipBase64', currentZipBase64);
    addHidden('Items[' + idx + '].ZipName', currentZipName);
    addHidden('Items[' + idx + '].IsClosed', 'false');
    addHidden('Items[' + idx + '].ServerName', serverName, 'fn_server_' + idx);
    addHidden('Items[' + idx + '].CharacterId', charId, 'fn_charId_' + idx);
    addHidden('Items[' + idx + '].CharacterPw', charPw, 'fn_charPw_' + idx);
    addHidden('Items[' + idx + '].CharacterMail', charMail, 'fn_charMail_' + idx);
    addHidden('Items[' + idx + '].CharacterMailPw', mailPw, 'fn_mailPw_' + idx);
    addHidden('Items[' + idx + '].OtpCode', otpCode, 'fn_otpCode_' + idx);
    addHidden('Items[' + idx + '].OtpPassword', otpPw, 'fn_otpPw_' + idx);

    // ── Sağ panel ────────────────────────────────────────────────────────
    var rightContainer = document.getElementById('rightPanelContainer');
    var panelDiv = document.createElement('div');
    panelDiv.id = 'rightPanel_' + idx;
    panelDiv.className = 'mb-3 p-2 rounded';
    panelDiv.style.cssText = 'background:#111118;border:1px solid #e84545;';
    panelDiv.innerHTML =
        '<div class="d-flex justify-content-between align-items-center mb-2">' +
        '<div>' +
        '<strong style="color:#fff;font-size:13px;">' + escHtml(opt.dataset.name) + '</strong><br>' +
        '<small class="text-muted">' + escHtml(opt.dataset.code) + '</small>' +
        '</div>' +
        '<span class="badge badge-warning">Acik</span>' +
        '</div>' +
        '<div style="font-size:12px;">' +
        buildEditField('Server Adi', 'fn_server_' + idx, serverName, false) +
        buildEditField('Karakter ID', 'fn_charId_' + idx, charId, false) +
        buildEditField('Karakter Sifresi', 'fn_charPw_' + idx, charPw, true, 'ep_charPw_' + idx) +
        buildEditField('Karakter Mail', 'fn_charMail_' + idx, charMail, false) +
        buildEditField('Mail Sifresi', 'fn_mailPw_' + idx, mailPw, true, 'ep_mailPw_' + idx) +
        buildEditField('OTP Kodu', 'fn_otpCode_' + idx, otpCode, false) +
        buildEditField('OTP Sifresi', 'fn_otpPw_' + idx, otpPw, true, 'ep_otpPw_' + idx) +
        '</div>';

    rightContainer.appendChild(panelDiv);

    newItemIndex++;
    document.getElementById('rowCount').textContent =
        document.querySelectorAll('#itemsTableBody tr').length;

    $('#addItemModal').modal('hide');
    resetModal();
}

// ── Edit field HTML ───────────────────────────────────────────────────────
function buildEditField(label, hiddenId, value, isPassword, inputId) {
    var safeVal = escHtml(value);
    var labelHtml =
        '<label style="color:#666;font-size:11px;text-transform:uppercase;letter-spacing:1px;">' +
        label + '</label>';

    if (!isPassword) {
        return '<div class="form-group mb-1">' + labelHtml +
            '<input type="text" class="form-control form-control-sm" value="' + safeVal + '"' +
            ' oninput="syncHidden(\'' + hiddenId + '\', this.value)" />' +
            '</div>';
    }

    return '<div class="form-group mb-1">' + labelHtml +
        '<div class="input-group input-group-sm">' +
        '<input type="password" class="form-control" id="' + inputId + '" value="' + safeVal + '"' +
        ' oninput="syncHidden(\'' + hiddenId + '\', this.value)" />' +
        '<div class="input-group-append">' +
        '<button type="button" class="btn btn-outline-secondary" tabindex="-1"' +
        ' onclick="toggleEditPw(\'' + inputId + '\', this)">' +
        '<i class="mdi mdi-eye"></i>' +
        '</button>' +
        '</div>' +
        '</div>' +
        '</div>';
}

// ── Modal temizle ─────────────────────────────────────────────────────────
function resetModal() {
    ['m_ProductId', 'm_Quantity', 'm_UnitPrice', 'm_ServerName',
        'm_CharacterId', 'm_CharacterPw', 'm_CharacterMail',
        'm_CharacterMailPw', 'm_OtpCode', 'm_OtpPassword'].forEach(function (id) {
            var el = document.getElementById(id);
            if (!el) return;
            el.value = id === 'm_Quantity' ? 1 : '';
            el.style.borderColor = '';
            if (id === 'm_CharacterPw' || id === 'm_OtpPassword' || id === 'm_CharacterMailPw') {
                el.type = 'password';
                var icon = el.parentElement.querySelector('.input-group-append i');
                if (icon) icon.className = 'mdi mdi-eye';
            }
        });
    var err = document.getElementById('charMailError');
    if (err) err.style.display = 'none';
    document.getElementById('m_ZipFile').value = '';
    var lbl = document.querySelector('label[for="m_ZipFile"]');
    if (lbl) lbl.textContent = 'Dosya sec...';
    document.getElementById('zipPreview').innerHTML = '';
    document.getElementById('zipProgress').style.display = 'none';
    document.getElementById('stockInfo').textContent = '';
    document.getElementById('stockWarning').style.display = 'none';
    currentZipBase64 = null;
    currentZipName = null;
}

// ── Yardimci ─────────────────────────────────────────────────────────────
function showErr(msg) {
    Swal.fire({
        title: 'Uyari!', text: msg, icon: 'warning',
        confirmButtonColor: '#e84545', background: '#1a1a2e', color: '#fff'
    });
}

function escHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/"/g, '&quot;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;');
}
