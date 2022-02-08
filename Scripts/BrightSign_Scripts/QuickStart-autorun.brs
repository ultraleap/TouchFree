' How to use:

' Set the width and height to match your monitor resolution
' NOTE: 2560x1440 resolution is unsupported, use 1280x720 for full screen display

' Rotate the content to portrait by uncommenting the "transform" if required
' Make sure the file is called "autorun.brs"
' Copy to the root of the SD card along with the "Tooling-Package"

Sub Main()
	x = 0
	y = 0
	width = 1920
	height = 1080
	rect = CreateObject("roRectangle", x, y, width, height)
	
	url = "file:/SD:/Tooling_Package/quick-start/Quick-Start_Example.html"
	
	config = {
		url: url,
		mouse_enabled: true,
		storage_path: "/local/",
		inspector_server: {
			port:2999
		}
		' transform: "rot90", ' Uncomment to rotate for portrait screens
	}
	
	html = CreateObject("roHtmlWidget", rect, config)
	sleep(1000)
	html.Show()

	while true
		' Do nothing
	end while
end Sub