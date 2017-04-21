$(document).ready(function () {
	/**
	 * Add active class to top nav link that corresponds to the page that is currently active
	 */
	function highlightActivePageLink() {
		var currentPage = window.location.pathname.replace(/^\//, "");

		var activeLink = $(".site-top-nav a").filter(function () {
			return $(this).data("url") == currentPage;
		});

		activeLink.first().parents("li").addClass("active");
	}

	highlightActivePageLink();

	/**
	 * Handle clicks on the main navigation links.
	 * Opens new window for link if you are on the game page.
	 */
	$(".site-top-nav a.link").on("click", function (e) {
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