$(document).ready(function () {
	var ws,
		userid = Cookies.get('ProvinceUserID'),
		protocol = location.protocol === "https:" ? "s" : "",
		gameInput = $("#game-input"),
		gameOutput = $("#game-output"),
		submitCmdBtn = $("#submit-cmd"),
		usedCommands = [],
		usedCommandPointer = null;

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

			gameInput.on("keyup", function gameInputMouseup(e) {
				if (e.keyCode == 38 || e.keyCode == 40) {
					e.preventDefault();
					handleUpDownArrows(e.keyCode);
				} else {
					usedCommandPointer = null;
				}
			});

			enableInput();
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
			disableInput();
		});

		ws.onclose = function () {
			gameOutput.append('Connection Closed - please refresh the screen to reconnect<br />');
			scrollToBottom();
			disableInput();
		};

		ws.onerror = function (e) {
			gameOutput.append('Oops something went wront <br/>');
			scrollToBottom();
			disableInput();
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
		var val = gameInput.val();

		if (val != '') {
			addToStack(usedCommands, val);
			ws.send(val);
			gameInput.val('');
		}
	};

	function scrollToBottom() {
		gameOutput.scrollTop(gameOutput[0].scrollHeight);
	};

	function disableInput() {
		$("#game-input").prop('disabled', true);
		$("#submit-cmd").prop('disabled', true);
	}

	function enableInput() {
		$("#game-input").prop('disabled', false);
		$("#submit-cmd").prop('disabled', false);
	}

	$(window).keydown(function (event) {
		if (event.keyCode == 13) {
			event.preventDefault();
			sendMessage();
			return false;
		};
	});

	function addToStack(stack, value) {
		if (stack.length >= 50) {
			stack.shift();
		}

		stack.push(value);
	}

	function getFromStack(stack, index) {

		if (typeof stack[index] === "undefined") {
			return null;
		}

		return stack[index];
	}

	function handleUpDownArrows(keyCode) {
		// 38 == up | 40 == down
		if (usedCommands.length < 1) {
			return;
		}

		if (usedCommandPointer == null) {
			if (keyCode == 40) {
				return;
			}

			usedCommandPointer = usedCommands.length - 1;
		} else {
			if (keyCode == 38) {
				usedCommandPointer = usedCommandPointer-- > 0 ? usedCommandPointer : 0;
			} else {
				usedCommandPointer = usedCommandPointer++ < usedCommands.length ? usedCommandPointer : usedCommands.length;
			}
		}

		var newVal = getFromStack(usedCommands, usedCommandPointer)

		gameInput.val(newVal);
	}
});