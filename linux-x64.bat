dotnet publish -c Release -r linux-x64 -f net6.0 --no-self-contained -o bundle/release/linux-x64
echo Creating archive...
tar.exe -a -c -f ./linux-x64-bundle.zip -C bundle/release/linux-x64 *