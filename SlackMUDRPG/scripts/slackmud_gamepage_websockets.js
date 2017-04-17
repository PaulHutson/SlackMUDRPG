$(document).ready(function () {
	var ws,
		userid = Cookies.get('ProvinceUserID'),
		protocol = location.protocol === "https:" ? "s" : "",
		gameInput = $("#game-input"),
		gameOutput = $("#game-output"),
		submitCmdBtn = $("#submit-cmd");

	if (userid !== null) {
		console.log("Cookie:" + userid);
		showGame();

		var baseurl = window.location.href,
			arr = baseurl.split("/"),
			url = 'ws' + protocol + '://' + arr[2] + '/Handlers/GameAccess.ashx?userID=' + userid;

		ws = new WebSocket(url);

		ws.onopen = function () {
			submitCmdBtn.click(function () {
				sendMessage();
				scrollToBottom();
			});
		};

		ws.onmessage = function (e) {
			if (e.data.substring(0, 27) == "You must create a character") {
				showLoginCreate()
			}
			gameOutput.append(e.data + '<br/>');
			scrollToBottom();
		};

		$('#cmdLeave').click(function () {
			ws.close();
			scrollToBottom();
		});

		ws.onclose = function () {
			gameOutput.append('Closed <br/>');
			scrollToBottom();
		};

		ws.onerror = function (e) {
			gameOutput.append('Oops something went wront <br/>');
			scrollToBottom();
		};
	} else {
		showLoginCreate();
	}

	function showGame() {
		$("#gameplay-section").show();
		$("#login-create-section").hide();
	};

	function showLoginCreate() {
		$("#gameplay-section").hide();
		$("#login-create-section").show();
	};

	function sendMessage() {
		ws.send(gameInput.val());
		gameInput.val('');
	};

	function scrollToBottom() {
		gameOutput.scrollTop(gameOutput[0].scrollHeight);
	};

	$(window).keydown(function (event) {
		if (event.keyCode == 13) {
			event.preventDefault();
			sendMessage();
			return false;
		};
	});
});