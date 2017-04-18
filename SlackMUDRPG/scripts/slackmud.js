$(document).ready(function () {

	/**
	 * Handle clicks on the main navigation links.
	 * Opens new window for link if you are on the game page.
	 */
	$(".site-top-nav a").on("click", function (e) {
		e.preventDefault();

		var dataUrl = $(this).data("url");

		if (window.location.pathname && window.location.pathname.match(/^\/Game\.aspx.*/)) {

			if (dataUrl.match(/game/i)) {
				return;
			}

			window.open(getUrlFromPath(dataUrl, "_blank"));
			return;
		}

		window.location.href = getUrlFromPath(dataUrl);
	});

	function getUrlFromPath(path) {
		return window.location.protocol + "//" + window.location.host + "/" + path;
	}
});