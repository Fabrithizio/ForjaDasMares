Add-Type -AssemblyName System.Drawing
$kitRoot = Split-Path -Parent $PSScriptRoot
$images = Get-ChildItem -LiteralPath (Join-Path $kitRoot 'Previews') -Filter '*.png' | Where-Object {$_.Name -notlike 'Prancha*'} | Sort-Object Name
$font = [System.Drawing.Font]::new('Segoe UI',13)
$headingFont = [System.Drawing.Font]::new('Segoe UI',21,[System.Drawing.FontStyle]::Bold)
for($page=0; $page -lt [math]::Ceiling($images.Count/12); $page++) {
 $canvas=[System.Drawing.Bitmap]::new(1440,1220)
 $graphics=[System.Drawing.Graphics]::FromImage($canvas)
 $graphics.Clear([System.Drawing.Color]::FromArgb(24,34,34))
 $graphics.DrawString(('KIT ORIGINAL - MODELOS 3D | PRANCHA '+($page+1)),$headingFont,[System.Drawing.Brushes]::Wheat,25,16)
 for($slot=0; $slot -lt 12; $slot++) {
  $index=$page*12+$slot
  if($index -ge $images.Count){break}
  $x=($slot%4)*360; $y=[math]::Floor($slot/4)*380+60
  $source=[System.Drawing.Image]::FromFile($images[$index].FullName)
  $graphics.DrawImage($source,$x+8,$y,344,344)
  $graphics.DrawString($images[$index].BaseName.Replace('_',' '),$font,[System.Drawing.Brushes]::White,$x+12,$y+345)
  $source.Dispose()
 }
 $canvas.Save((Join-Path $kitRoot ('Previews\Prancha_'+($page+1)+'.png')),[System.Drawing.Imaging.ImageFormat]::Png)
 $graphics.Dispose();$canvas.Dispose()
}
$font.Dispose();$headingFont.Dispose()
