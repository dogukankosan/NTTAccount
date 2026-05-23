var itemIndex = 0;
var draftItems = [];
var currentZipBase64 = null;
var currentZipName = null;
var isLoading = false;

// Excel'den gelen satırlar için bekleyen ZIP'ler
// { idx, zipBase64, zipName } — null ise henüz yüklenmedi
var pendingZips = {};

// ── Sayfa yüklenince ──────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', function () {
    loadDraft();

    document.getElementById('orderForm').addEventListener('submit', function (e) {
        if (!document.getElementById('f_UserId').value) {
            e.preventDefault();
            showErr('Lütfen listeden bir kullanıcı seçiniz!');
            return;
        }
        if (draftItems.length === 0) {
            e.preventDefault();
            showErr('En az bir sipariş satırı eklemelisiniz.');
            return;
        }

        // ZIP kontrolü — tüm satırların ZIP'i var mı?
        var missingZip = draftItems.find(function (item) {
            return !item.zipBase64;
        });
        if (missingZip) {
            e.preventDefault();
            showErr('Tüm satırlar için ZIP dosyası yüklenmelidir!');
            return;
        }

        localStorage.removeItem(DRAFT_KEY);
        var btn = document.getElementById('saveBtn');
        btn.disabled = true;
        btn.innerHTML = '<i class="mdi mdi-loading mdi-spin mr-1"></i> Kaydediliyor...';
    });

    document.getElementById('f_Note').addEventListener('input', saveDraft);

    document.addEventListener('click', function (e) {
        var wrap = document.querySelector('.user-search-wrap');
        if (wrap && !wrap.contains(e.target)) {
            var dd = document.getElementById('userDropdown');
            if (dd) dd.style.display = 'none';
        }
    });

    var userSearchEl = document.getElementById('userSearch');
    if (userSearchEl) {
        userSearchEl.addEventListener('input', filterUsers);
    }
});
function fixDecimal(input) {
    input.value = input.value.replace(',', '.');
    input.value = input.value.replace(/[^0-9.]/g, '');
}
// ── Kullanıcı arama ───────────────────────────────────────────────────────
function filterUsers() {
    var search = document.getElementById('userSearch').value.trim().toLowerCase();
    var dropdown = document.getElementById('userDropdown');
    var list = document.getElementById('userDropdownList');
    var empty = document.getElementById('userDropdownEmpty');

    if (search.length < 1) {
        dropdown.style.display = 'none';
        return;
    }

    var filtered = allUsers.filter(function (u) {
        return u.email.toLowerCase().includes(search);
    });

    list.innerHTML = '';
    dropdown.style.display = 'block';

    if (filtered.length === 0) {
        empty.style.display = 'block';
        list.style.display = 'none';
    } else {
        empty.style.display = 'none';
        list.style.display = 'block';
        filtered.forEach(function (u) {
            var item = document.createElement('div');
            item.style.cssText =
                'padding:10px 14px;cursor:pointer;border-bottom:1px solid #1e1e2e;' +
                'color:#ccc;font-size:13px;transition:background 0.2s;';
            item.innerHTML =
                '<i class="mdi mdi-account mr-2" style="color:#e84545;"></i>' +
                highlightMatch(u.email, search);
            item.onmouseover = function () {
                this.style.background = '#1a1a2e';
                this.style.color = '#fff';
            };
            item.onmouseout = function () {
                this.style.background = '';
                this.style.color = '#ccc';
            };
            item.onclick = function () { selectUser(u.id, u.email, true); };
            list.appendChild(item);
        });
    }
}

function highlightMatch(email, search) {
    var idx = email.toLowerCase().indexOf(search);
    if (idx === -1) return email;
    return email.substring(0, idx) +
        '<strong style="color:#e84545;">' +
        email.substring(idx, idx + search.length) +
        '</strong>' +
        email.substring(idx + search.length);
}

function selectUser(id, email, save) {
    document.getElementById('f_UserId').value = id;
    document.getElementById('userSearch').value = '';
    document.getElementById('userSearch').style.display = 'none';
    document.getElementById('userDropdown').style.display = 'none';
    document.getElementById('userSearchError').style.display = 'none';
    document.getElementById('selectedUserEmail').textContent = email;
    document.getElementById('selectedUserBadge').style.display = 'block';
    if (save && !isLoading) saveDraft();
}

function clearUser() {
    document.getElementById('f_UserId').value = '';
    document.getElementById('selectedUserBadge').style.display = 'none';
    document.getElementById('userSearch').style.display = 'block';
    document.getElementById('userSearch').value = '';
    document.getElementById('userSearch').focus();
    if (!isLoading) saveDraft();
}

// ── Taslak kaydet ─────────────────────────────────────────────────────────
function saveDraft() {
    if (isLoading) return;
    var draft = {
        userId: document.getElementById('f_UserId').value,
        userEmail: document.getElementById('selectedUserEmail').textContent,
        note: document.getElementById('f_Note').value,
        items: draftItems
    };
    try {
        localStorage.setItem(DRAFT_KEY, JSON.stringify(draft));
    } catch (e) {
        console.warn('Draft kayit hatasi:', e);
    }
}

// ── Taslağı yükle ─────────────────────────────────────────────────────────
function loadDraft() {
    var raw = localStorage.getItem(DRAFT_KEY);
    if (!raw) return;
    try {
        isLoading = true;
        var draft = JSON.parse(raw);

        if (draft.userId && draft.userEmail) {
            document.getElementById('f_UserId').value = draft.userId;
            document.getElementById('selectedUserEmail').textContent = draft.userEmail;
            document.getElementById('selectedUserBadge').style.display = 'block';
            document.getElementById('userSearch').style.display = 'none';
        }

        if (draft.note) {
            document.getElementById('f_Note').value = draft.note;
        }

        if (draft.items && draft.items.length > 0) {
            draft.items.forEach(function (item) {
                draftItems.push(item);
                renderRow(item);
                buildHiddenInputs(item);
                itemIndex = Math.max(itemIndex, item.idx + 1);
            });
            document.getElementById('rowCount').textContent = draftItems.length;
            document.getElementById('draftBanner').style.display = 'block';
        }
    } catch (e) {
        localStorage.removeItem(DRAFT_KEY);
    } finally {
        isLoading = false;
    }
}

// ── Taslağı temizle ───────────────────────────────────────────────────────
function clearDraft() {
    Swal.fire({
        title: 'Taslağı Temizle',
        text: 'Tüm satırlar silinecek. Emin misiniz?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Evet, Temizle',
        cancelButtonText: 'İptal',
        confirmButtonColor: '#e84545',
        cancelButtonColor: '#444',
        background: '#1a1a2e',
        color: '#fff'
    }).then(function (r) {
        if (!r.isConfirmed) return;
        localStorage.removeItem(DRAFT_KEY);
        draftItems = [];
        itemIndex = 0;
        pendingZips = {};
        document.getElementById('itemsTableBody').innerHTML =
            '<tr id="emptyRow"><td colspan="7" class="text-center text-muted py-3">' +
            '<i class="mdi mdi-information-outline mr-1"></i>' +
            'Henüz satır eklenmedi.</td></tr>';
        document.getElementById('hiddenInputs').innerHTML = '';
        document.getElementById('rowCount').textContent = '0';
        document.getElementById('draftBanner').style.display = 'none';
        document.getElementById('excelErrorPanel').style.display = 'none';
        document.getElementById('f_Note').value = '';
        clearUser();
    });
}

// ── Satırı tabloya render et ──────────────────────────────────────────────
// Satır listesinde ZIP yoksa buton göster
function renderRow(item) {
    var tbody = document.getElementById('itemsTableBody');
    var emptyRow = document.getElementById('emptyRow');
    if (emptyRow) emptyRow.remove();

    var zipCell = item.zipBase64
        ? '<span class="badge badge-success"><i class="mdi mdi-check-circle mr-1"></i>' +
        escHtml(item.zipName) + '</span>'
        : '<button type="button" class="btn btn-warning btn-sm" onclick="openZipModal()">' +
        '<i class="mdi mdi-upload mr-1"></i>ZIP Seç</button>';

    var tr = document.createElement('tr');
    tr.id = 'row_' + item.idx;
    tr.innerHTML =
        '<td>' + (tbody.children.length + 1) + '</td>' +
        '<td><strong>' + escHtml(item.productName) + '</strong><br>' +
        '<small class="text-muted">' + escHtml(item.productCode) + '</small></td>' +
        '<td>' + escHtml(item.serverName) + '</td>' +
        '<td>' + item.quantity + '</td>' +
        '<td>' + parseFloat(item.unitPrice || 0).toFixed(2) + ' ₺</td>' +
        '<td id="zipCell_' + item.idx + '">' + zipCell + '</td>' +
        '<td><button type="button" class="btn btn-danger btn-sm" ' +
        'onclick="removeRow(' + item.idx + ')">' +
        '<i class="mdi mdi-delete"></i></button></td>';
    tbody.appendChild(tr);
}
// ── Gizli inputları ekle ──────────────────────────────────────────────────
function buildHiddenInputs(item) {
    var hidden = document.getElementById('hiddenInputs');
    hidden.querySelectorAll('[data-item-idx="' + item.idx + '"]')
        .forEach(function (el) { el.remove(); });

    var fields = {
        ProductId: item.productId,
        Quantity: item.quantity,
        UnitPrice: String(item.unitPrice || '0').replace(',', '.'), // ✅
        ServerName: item.serverName,
        CharacterId: item.characterId,
        CharacterPw: item.characterPw,
        CharacterMail: item.characterMail,
        CharacterMailPw: item.characterMailPw,
        OtpCode: item.otpCode,
        OtpPassword: item.otpPassword,
        ZipBase64: item.zipBase64 || '',
        ZipName: item.zipName || ''
    };

    Object.keys(fields).forEach(function (key) {
        var input = document.createElement('input');
        input.type = 'hidden';
        input.name = 'Items[' + item.idx + '].' + key;
        input.value = fields[key] || '';
        input.setAttribute('data-item-idx', item.idx);
        hidden.appendChild(input);
    });
}

// ── Gizli inputlardaki ZIP'i güncelle ────────────────────────────────────
function updateHiddenZip(idx, zipBase64, zipName) {
    var hidden = document.getElementById('hiddenInputs');

    var b64Input = hidden.querySelector('[name="Items[' + idx + '].ZipBase64"]');
    if (b64Input) b64Input.value = zipBase64;

    var nameInput = hidden.querySelector('[name="Items[' + idx + '].ZipName"]');
    if (nameInput) nameInput.value = zipName;
}

// ── Modal: Fiyat doldur ───────────────────────────────────────────────────
function modalFillPrice() {
    var sel = document.getElementById('m_ProductId');
    var opt = sel.options[sel.selectedIndex];
    if (opt && opt.dataset.price) {
        var priceInput = document.getElementById('m_UnitPrice');
        priceInput.value = parseFloat(opt.dataset.price).toFixed(2); // ✅ noktalı gelir
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

// ── Karakter Mail validasyon ──────────────────────────────────────────────
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

// ── ZIP seçilince base64'e çevir ──────────────────────────────────────────
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
            title: 'Geçersiz Dosya!',
            text: 'Sadece .zip dosyası yükleyebilirsiniz.',
            icon: 'error',
            confirmButtonColor: '#e84545',
            background: '#1a1a2e',
            color: '#fff'
        });
        input.value = '';
        label.textContent = 'Dosya seç...';
        return;
    }

    if (file.size > 10 * 1024 * 1024) {
        Swal.fire({
            title: 'Dosya Çok Büyük!',
            text: 'Maksimum 10MB yükleyebilirsiniz.',
            icon: 'warning',
            confirmButtonColor: '#e84545',
            background: '#1a1a2e',
            color: '#fff'
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
            escHtml(file.name) + ' — ' +
            (file.size / 1024).toFixed(1) + ' KB hazır</span>';
    };
    reader.onerror = function () {
        progress.style.display = 'none';
        Swal.fire({
            title: 'Okuma Hatası!',
            text: 'Dosya okunamadı, tekrar deneyin.',
            icon: 'error',
            confirmButtonColor: '#e84545',
            background: '#1a1a2e',
            color: '#fff'
        });
    };
    reader.readAsDataURL(file);
}

// ── Modaldan satır ekle ───────────────────────────────────────────────────
function addItemFromModal() {
    var sel = document.getElementById('m_ProductId');
    var opt = sel.options[sel.selectedIndex];
    var qty = parseInt(document.getElementById('m_Quantity').value) || 0;
    var stock = parseInt(opt.dataset.stock) || 0;
    var charMail = document.getElementById('m_CharacterMail').value.trim();
    var emailPattern = /^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$/;

    if (!sel.value) { showErr('Ürün seçiniz!'); return; }
    if (qty < 1) { showErr('Miktar en az 1 olmalıdır!'); return; }
    if (qty > stock) { showErr('Yetersiz stok! Mevcut: ' + stock); return; }
    if (!document.getElementById('m_ServerName').value.trim()) { showErr('Server adı zorunludur!'); return; }
    if (!document.getElementById('m_CharacterId').value.trim()) { showErr('Karakter ID zorunludur!'); return; }
    if (!document.getElementById('m_CharacterPw').value.trim()) { showErr('Karakter şifresi zorunludur!'); return; }
    if (!charMail) { showErr('Karakter mail zorunludur!'); return; }
    if (!emailPattern.test(charMail)) { showErr('Geçerli bir karakter mail giriniz!'); return; }
    if (!document.getElementById('m_CharacterMailPw').value.trim()) { showErr('Mail şifresi zorunludur!'); return; }
    if (!document.getElementById('m_OtpCode').value.trim()) { showErr('OTP kodu zorunludur!'); return; }
    if (!document.getElementById('m_OtpPassword').value.trim()) { showErr('OTP şifresi zorunludur!'); return; }
    if (!currentZipBase64) { showErr('ZIP dosyası zorunludur!'); return; }

    var item = {
        idx: itemIndex,
        productId: sel.value,
        productName: opt.dataset.name,
        productCode: opt.dataset.code,
        quantity: qty,
        unitPrice: (document.getElementById('m_UnitPrice').value || '0').replace(',', '.'), // ✅


        serverName: document.getElementById('m_ServerName').value.trim(),
        characterId: document.getElementById('m_CharacterId').value.trim(),
        characterPw: document.getElementById('m_CharacterPw').value.trim(),
        characterMail: charMail,
        characterMailPw: document.getElementById('m_CharacterMailPw').value.trim(),
        otpCode: document.getElementById('m_OtpCode').value.trim(),
        otpPassword: document.getElementById('m_OtpPassword').value.trim(),
        zipBase64: currentZipBase64,
        zipName: currentZipName
    };

    draftItems.push(item);
    renderRow(item);
    buildHiddenInputs(item);
    itemIndex++;
    document.getElementById('rowCount').textContent = draftItems.length;
    saveDraft(); updateZipBtn(); // ✅
    $('#addItemModal').modal('hide');
    resetModal();
}

// ── Satır sil ─────────────────────────────────────────────────────────────
function removeRow(idx) {
    var row = document.getElementById('row_' + idx);
    if (row) row.remove();
    document.getElementById('hiddenInputs')
        .querySelectorAll('[data-item-idx="' + idx + '"]')
        .forEach(function (el) { el.remove(); });
    draftItems = draftItems.filter(function (i) { return i.idx !== idx; });
    delete pendingZips[idx];
    saveDraft();
    var tbody = document.getElementById('itemsTableBody');
    if (tbody.children.length === 0) {
        tbody.innerHTML =
            '<tr id="emptyRow"><td colspan="7" class="text-center text-muted py-3">' +
            '<i class="mdi mdi-information-outline mr-1"></i>' +
            'Henüz satır eklenmedi.</td></tr>';
    }
    document.getElementById('rowCount').textContent = draftItems.length; updateZipBtn(); // ✅
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
            // ✅ CharacterMailPw de eklendi
            if (id === 'm_CharacterPw' || id === 'm_OtpPassword' || id === 'm_CharacterMailPw') {
                el.type = 'password';
                var icon = el.parentElement.querySelector('.input-group-append i');
                if (icon) icon.className = 'mdi mdi-eye';
            }
        });
    var charMailErr = document.getElementById('charMailError');
    if (charMailErr) charMailErr.style.display = 'none';
    document.getElementById('m_ZipFile').value = '';
    var lbl = document.querySelector('label[for="m_ZipFile"]');
    if (lbl) lbl.textContent = 'Dosya seç...';
    document.getElementById('zipPreview').innerHTML = '';
    document.getElementById('zipProgress').style.display = 'none';
    document.getElementById('stockInfo').textContent = '';
    document.getElementById('stockWarning').style.display = 'none';
    currentZipBase64 = null;
    currentZipName = null;
}

// ════════════════════════════════════════════════════════════════════════════
// ── EXCEL İŞLEMLERİ ────────────────────────────────────────────────────────
// ════════════════════════════════════════════════════════════════════════════

// ── Şablon İndir ─────────────────────────────────────────────────────────
function downloadTemplate() {
    var wb = XLSX.utils.book_new();
    var headers = [
        'Malzeme Kodu',
        'Miktar',
        'Birim Fiyat',
        'Server Adı',
        'Karakter ID',
        'Karakter Şifresi',
        'Karakter Mail',
        'Mail Şifresi',
        'OTP Kodu',
        'OTP Şifresi'
    ];

    // Örnek satır
    var example = [
        'NTT-001',
        1,
        '',
        'Apex',
        'KarakterAdi',
        'Sifre123',
        'ornek@mail.com',
        'MailSifre123',
        '862953',
        'OtpSifre123'
    ];

    var ws = XLSX.utils.aoa_to_sheet([headers, example]);

    // Kolon genişlikleri
    ws['!cols'] = [
        { wch: 15 }, { wch: 8 }, { wch: 12 }, { wch: 15 },
        { wch: 15 }, { wch: 15 }, { wch: 25 }, { wch: 15 },
        { wch: 12 }, { wch: 15 }
    ];

    XLSX.utils.book_append_sheet(wb, ws, 'Sipariş');
    XLSX.writeFile(wb, 'SiparisSablonu.xlsx');
}

// ── Excel Yükle ───────────────────────────────────────────────────────────
function handleExcelUpload(input) {
    if (!input.files || input.files.length === 0) return;
    var file = input.files[0];

    // Sadece xlsx/xls
    var ext = file.name.toLowerCase();
    if (!ext.endsWith('.xlsx') && !ext.endsWith('.xls')) {
        showErr('Sadece .xlsx veya .xls dosyası yükleyebilirsiniz!');
        input.value = '';
        return;
    }

    var reader = new FileReader();
    reader.onload = function (e) {
        try {
            var data = new Uint8Array(e.target.result);
            var wb = XLSX.read(data, { type: 'array' });
            var ws = wb.Sheets[wb.SheetNames[0]];
            var rows = XLSX.utils.sheet_to_json(ws, { header: 1, defval: '' });

            // İlk satır header — 2. satırdan başla
            if (rows.length < 2) {
                showErr('Excel dosyası boş veya sadece başlık satırı var!');
                input.value = '';
                return;
            }

            var errors = [];
            var validItems = [];
            var emailPattern = /^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$/;

            for (var i = 1; i < rows.length; i++) {
                var row = rows[i];
                var excelRowNo = i + 1;

                // Tamamen boş satırı atla
                var allEmpty = row.every(function (c) { return String(c).trim() === ''; });
                if (allEmpty) continue;

                var code = String(row[0] || '').trim().toUpperCase();
                var qty = parseInt(row[1]) || 0;
                var unitPrice = String(row[2] || '').trim();
                var serverName = String(row[3] || '').trim();
                var charId = String(row[4] || '').trim();
                var charPw = String(row[5] || '').trim();
                var charMail = String(row[6] || '').trim();
                var mailPw = String(row[7] || '').trim();
                var otpCode = String(row[8] || '').trim();
                var otpPassword = String(row[9] || '').trim();

                // Miktar 0 ise satırı atla (hata değil)
                // Miktar 0 ise atla (hata değil)
           

                // Miktar negatif veya boşsa hata ver
                if (!row[1] && row[1] !== 0) {
                    errors.push({ row: excelRowNo, code: code, msg: 'Miktar boş olamaz.' });
                    continue;
                }
                if (qty <= 0 ) {
                    errors.push({ row: excelRowNo, code: code, msg: 'Miktar en az 1 olmalıdır. Girilen: ' + row[1] });
                    continue;
                }

                // Zorunlu alan kontrolleri
                if (!code) { errors.push({ row: excelRowNo, code: code || '-', msg: 'Malzeme kodu boş.' }); continue; }
                if (qty < 0) { errors.push({ row: excelRowNo, code: code, msg: 'Miktar negatif olamaz.' }); continue; }
                if (!serverName) { errors.push({ row: excelRowNo, code: code, msg: 'Server adı boş.' }); continue; }
                if (!charId) { errors.push({ row: excelRowNo, code: code, msg: 'Karakter ID boş.' }); continue; }
                if (!charPw) { errors.push({ row: excelRowNo, code: code, msg: 'Karakter şifresi boş.' }); continue; }
                if (!charMail) { errors.push({ row: excelRowNo, code: code, msg: 'Karakter mail boş.' }); continue; }
                if (!emailPattern.test(charMail)) { errors.push({ row: excelRowNo, code: code, msg: 'Karakter mail geçersiz: ' + charMail }); continue; }
                if (!mailPw) { errors.push({ row: excelRowNo, code: code, msg: 'Mail şifresi boş.' }); continue; }
                if (!otpCode) { errors.push({ row: excelRowNo, code: code, msg: 'OTP kodu boş.' }); continue; }
                if (!otpPassword) { errors.push({ row: excelRowNo, code: code, msg: 'OTP şifresi boş.' }); continue; }

                // Ürün bul — aktif ürünler içinde ara
                var product = allProducts.find(function (p) {
                    return p.code === code;
                });

                if (!product) {
                    errors.push({ row: excelRowNo, code: code, msg: 'Malzeme kodu bulunamadı veya pasif.' });
                    continue;
                }

                // Stok kontrolü
                if (qty > product.stock) {
                    errors.push({ row: excelRowNo, code: code, msg: 'Yetersiz stok! Mevcut: ' + product.stock + ', İstenen: ' + qty });
                    continue;
                }

                // Birim fiyat — boşsa default fiyat
                var finalPrice = unitPrice !== '' ? parseFloat(unitPrice) : product.price;
                if (isNaN(finalPrice) || finalPrice < 0) {
                    errors.push({ row: excelRowNo, code: code, msg: 'Birim fiyat geçersiz.' });
                    continue;
                }

                validItems.push({
                    excelRow: excelRowNo,
                    productId: String(product.id),
                    productName: product.name,
                    productCode: product.code,
                    quantity: qty,
                    unitPrice: finalPrice,
                    serverName: serverName,
                    characterId: charId,
                    characterPw: charPw,
                    characterMail: charMail,
                    characterMailPw: mailPw,
                    otpCode: otpCode,
                    otpPassword: otpPassword,
                    zipBase64: null,
                    zipName: null
                });
            }

            // Herhangi bir hata varsa hiçbir şey aktarma
            if (errors.length > 0) {
                showExcelErrors(errors);
                input.value = '';
                return;
            }

            if (validItems.length === 0) {
                showErr('Aktarılacak geçerli satır bulunamadı! (Miktar 0 olan satırlar atlandı)');
                input.value = '';
                return;
            }

            // Hata yok — hata panelini gizle
            document.getElementById('excelErrorPanel').style.display = 'none';

            // Satırları ekle
            validItems.forEach(function (vi) {
                var item = {
                    idx: itemIndex,
                    productId: vi.productId,
                    productName: vi.productName,
                    productCode: vi.productCode,
                    quantity: vi.quantity,
                    unitPrice: vi.unitPrice,
                    serverName: vi.serverName,
                    characterId: vi.characterId,
                    characterPw: vi.characterPw,
                    characterMail: vi.characterMail,
                    characterMailPw: vi.characterMailPw,
                    otpCode: vi.otpCode,
                    otpPassword: vi.otpPassword,
                    zipBase64: null,
                    zipName: null
                };
                draftItems.push(item);
                renderRow(item);
                buildHiddenInputs(item);
                itemIndex++;
            });

            document.getElementById('rowCount').textContent = draftItems.length;
            saveDraft(); updateZipBtn(); // ✅
            input.value = '';

            // ZIP yükleme modalını aç
            Swal.fire({
                title: validItems.length + ' satır aktarıldı!',
                html: 'Şimdi her satır için ZIP dosyasını yükleyiniz.',
                icon: 'success',
                confirmButtonText: 'ZIP Yükle',
                confirmButtonColor: '#28a745',
                background: '#1a1a2e',
                color: '#fff'
            }).then(function () {
                openZipModal();
            });

        } catch (ex) {
            showErr('Excel okunurken hata oluştu: ' + ex.message);
            input.value = '';
        }
    };
    reader.readAsArrayBuffer(file);
}
// addItemFromModal ve handleExcelUpload sonrasına ekle
function updateZipBtn() {
    var hasNoZip = draftItems.some(function (i) { return !i.zipBase64; });
    var btn = document.getElementById('zipModalBtn');
    if (btn) btn.style.display = hasNoZip ? 'inline-block' : 'none';
}
// ── Excel Hata Paneli Göster ──────────────────────────────────────────────
function showExcelErrors(errors) {
    var panel = document.getElementById('excelErrorPanel');
    var tbody = document.getElementById('excelErrorBody');
    tbody.innerHTML = '';

    errors.forEach(function (err) {
        var tr = document.createElement('tr');
        tr.innerHTML =
            '<td>' + err.row + '. Satır</td>' +
            '<td>' + escHtml(err.code) + '</td>' +
            '<td>' + escHtml(err.msg) + '</td>';
        tbody.appendChild(tr);
    });

    panel.style.display = 'block';
    panel.scrollIntoView({ behavior: 'smooth' });
}

// ── ZIP Yükleme Modalı ────────────────────────────────────────────────────
function openZipModal() {
    // ZIP'i olmayan satırları bul
    var needsZip = draftItems.filter(function (item) { return !item.zipBase64; });

    if (needsZip.length === 0) {
        Swal.fire({
            title: 'Tüm ZIP\'ler Yüklü!',
            text: 'Tüm satırların ZIP dosyaları mevcut.',
            icon: 'success',
            confirmButtonColor: '#28a745',
            background: '#1a1a2e',
            color: '#fff'
        });
        return;
    }

    var list = document.getElementById('zipUploadList');
    list.innerHTML = '';

    needsZip.forEach(function (item) {
        var div = document.createElement('div');
        div.className = 'mb-3 p-3 rounded';
        div.style.cssText = 'background:#111118;border:1px solid #1e1e2e;';
        div.id = 'zipRow_' + item.idx;
        div.innerHTML =
            '<div class="d-flex justify-content-between align-items-center mb-2">' +
            '<div>' +
            '<strong style="color:#fff;">' + escHtml(item.productName) + '</strong>' +
            '<span class="badge badge-warning ml-2">' + escHtml(item.productCode) + '</span><br>' +
            '<small style="color:#666;">Server: ' + escHtml(item.serverName) + ' | Miktar: ' + item.quantity + '</small>' +
            '</div>' +
            '<span class="badge badge-warning" id="zipStatus_' + item.idx + '">ZIP Bekleniyor</span>' +
            '</div>' +
            '<div class="custom-file">' +
            '<input type="file" class="custom-file-input" id="zipInput_' + item.idx + '" ' +
            'accept=".zip" onchange="handleModalZipSelect(this, ' + item.idx + ')" />' +
            '<label class="custom-file-label text-muted" for="zipInput_' + item.idx + '">Dosya seç...</label>' +
            '</div>' +
            '<div id="zipRowProgress_' + item.idx + '" class="mt-2" style="display:none;">' +
            '<div class="progress" style="height:4px;"><div class="progress-bar bg-success ' +
            'progress-bar-striped progress-bar-animated" style="width:100%"></div></div>' +
            '<small class="text-muted">İşleniyor...</small>' +
            '</div>';
        list.appendChild(div);
    });

    $('#zipUploadModal').modal('show');
}

// ── Modal ZIP Seç ─────────────────────────────────────────────────────────
function handleModalZipSelect(input, idx) {
    var label = input.nextElementSibling;
    var progress = document.getElementById('zipRowProgress_' + idx);
    var status = document.getElementById('zipStatus_' + idx);

    if (!input.files || input.files.length === 0) return;
    var file = input.files[0];

    if (!file.name.toLowerCase().endsWith('.zip')) {
        Swal.fire({
            title: 'Geçersiz Dosya!',
            text: 'Sadece .zip dosyası yükleyebilirsiniz.',
            icon: 'error',
            confirmButtonColor: '#e84545',
            background: '#1a1a2e',
            color: '#fff'
        });
        input.value = '';
        return;
    }

    if (file.size > 10 * 1024 * 1024) {
        Swal.fire({
            title: 'Dosya Çok Büyük!',
            text: 'Maksimum 10MB yükleyebilirsiniz.',
            icon: 'warning',
            confirmButtonColor: '#e84545',
            background: '#1a1a2e',
            color: '#fff'
        });
        input.value = '';
        return;
    }

    label.textContent = file.name;
    if (progress) progress.style.display = 'block';
    if (status) {
        status.className = 'badge badge-warning';
        status.textContent = 'İşleniyor...';
    }

    var reader = new FileReader();
    reader.onload = function (e) {
        var base64 = e.target.result;

        // draftItems'da güncelle
        var item = draftItems.find(function (i) { return i.idx === idx; });
        if (item) {
            item.zipBase64 = base64;
            item.zipName = file.name;
        }

        // Hidden input'u güncelle
        updateHiddenZip(idx, base64, file.name);

        // Tablodaki badge'i güncelle
        var zipCell = document.getElementById('zipCell_' + idx);
        if (zipCell) {
            zipCell.innerHTML =
                '<span class="badge badge-success">' +
                '<i class="mdi mdi-check-circle mr-1"></i>' +
                escHtml(file.name) + '</span>';
        }

        if (progress) progress.style.display = 'none';
        if (status) {
            status.className = 'badge badge-success';
            status.textContent = 'Yüklendi!';
        }

        // Satır arka planını güncelle
        var row = document.getElementById('zipRow_' + idx);
        if (row) row.style.borderColor = '#28a745';

        saveDraft(); updateZipBtn(); // ✅
    };
    reader.onerror = function () {
        if (progress) progress.style.display = 'none';
        if (status) {
            status.className = 'badge badge-danger';
            status.textContent = 'Hata!';
        }
        Swal.fire({
            title: 'Okuma Hatası!',
            text: 'Dosya okunamadı, tekrar deneyin.',
            icon: 'error',
            confirmButtonColor: '#e84545',
            background: '#1a1a2e',
            color: '#fff'
        });
    };
    reader.readAsDataURL(file);
}

// ── ZIP Modal Onayla ──────────────────────────────────────────────────────
function confirmZipUploads() {
    var needsZip = draftItems.filter(function (item) { return !item.zipBase64; });

    if (needsZip.length > 0) {
        Swal.fire({
            title: 'Eksik ZIP!',
            html: needsZip.length + ' satır için ZIP yüklenmedi.<br>' +
                '<small>Tüm ZIP\'leri yüklemeden kaydedilemez.</small>',
            icon: 'warning',
            confirmButtonColor: '#e84545',
            background: '#1a1a2e',
            color: '#fff'
        });
        return;
    }

    $('#zipUploadModal').modal('hide');
    Swal.fire({
        title: 'Tüm ZIP\'ler Yüklendi!',
        text: 'Siparişi kaydedebilirsiniz.',
        icon: 'success',
        confirmButtonColor: '#28a745',
        background: '#1a1a2e',
        color: '#fff'
    });
}

// ── Yardımcı ─────────────────────────────────────────────────────────────
function showErr(msg) {
    Swal.fire({
        title: 'Uyarı!',
        text: msg,
        icon: 'warning',
        confirmButtonColor: '#e84545',
        background: '#1a1a2e',
        color: '#fff'
    });
}

function escHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;').replace(/"/g, '&quot;')
        .replace(/</g, '&lt;').replace(/>/g, '&gt;');
}