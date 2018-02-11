for /R "_site" %%x in (*.html)     do move /y "%%x" "%%~fx.partial"
for /R "_site" %%x in (*.html.tmp) do move /y "%%x" "%%~dpnx"

