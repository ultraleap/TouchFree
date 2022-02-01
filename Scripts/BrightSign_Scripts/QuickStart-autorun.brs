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
	}
	
	html = CreateObject("roHtmlWidget", rect, config)
	sleep(1000)
	html.Show()

	while true
		' Do nothing
	end while
end Sub