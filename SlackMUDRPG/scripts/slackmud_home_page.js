"use strict"

$(function () {
	resizePlayLinks();

	$(window).resize(function () {
		resizePlayLinks()
	});

	function resizePlayLinks() {
		var width = $(window).width();

		if (width > 767) {
			var targetHeight = $(".content-block:first").outerHeight();
			$(".slackmud-list-group").css("height", targetHeight + "px");
		} else {
			$(".slackmud-list-group").css("height", "auto");
		}
	}
});