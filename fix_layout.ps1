$f = 'c:\Users\User\Desktop\PS-revit-addin\Forms\MainForm.cs'
$content = [System.IO.File]::ReadAllText($f, [System.Text.Encoding]::UTF8)

# RefreshProductCards 핵심 레이아웃 변경
# TopDown + 높이 3등분  →  LeftToRight + 너비 3등분, 높이는 전체
$old = '            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.WrapContents = false;
            flowLayoutPanel1.AutoScroll = false;
            flowLayoutPanel1.Padding = new Padding(0);

            int totalH = flowLayoutPanel1.ClientSize.Height > 100 ? flowLayoutPanel1.ClientSize.Height : 548;
            int totalW = flowLayoutPanel1.ClientSize.Width > 100 ? flowLayoutPanel1.ClientSize.Width : 674;
            int secH = (totalH - (VendorNames.Length - 1) * 2) / VendorNames.Length;
            int secW = totalW - 4;'

$new = '            flowLayoutPanel1.FlowDirection = FlowDirection.LeftToRight;
            flowLayoutPanel1.WrapContents = false;
            flowLayoutPanel1.AutoScroll = false;
            flowLayoutPanel1.Padding = new Padding(0);

            int totalH = flowLayoutPanel1.ClientSize.Height > 100 ? flowLayoutPanel1.ClientSize.Height : 548;
            int totalW = flowLayoutPanel1.ClientSize.Width > 100 ? flowLayoutPanel1.ClientSize.Width : 674;
            int secW = (totalW - (VendorNames.Length - 1) * 2) / VendorNames.Length;
            int secH = totalH - 4;'

if ($content.Contains($old)) {
    $content = $content.Replace($old, $new)
    Write-Host "RefreshProductCards layout changed"
} else {
    Write-Host "Pattern NOT found"
}

[System.IO.File]::WriteAllText($f, $content, [System.Text.Encoding]::UTF8)
Write-Host "Saved"
