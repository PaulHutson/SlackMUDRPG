"use strict"

$(function () {
	$(".side-nav a").filter(":first").addClass("active");

	$(".side-nav a").on("click", function (e) {
		e.preventDefault();

		var group = $(this).data("group") || null;

		if (group !== null) {
			$(".game-info-accordion .info-group").hide();
			$(".game-info-accordion #" + group).show();
			$(".game-info-accordion .panel-collapse.in").collapse("hide");
			$(".side-nav a").removeClass("active");
			$(this).addClass("active");
		}
	});
});