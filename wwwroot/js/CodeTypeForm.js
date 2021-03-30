var input = document.getElementById('pin_input');

var checkCode = function () {
	var $errorLabel = $('#Lbl_Error');
	var $pinborder = $(input).closest(".pin_border");
	$pinborder.removeClass("wrong");

	if ($pinborder.is(".wait"))
		return;
	var typedCode = $('#pin_input').val();
	var check_info = $('#check_info').val(); 
	if (typedCode.length == 4) {
		$pinborder.addClass("wait");
		$errorLabel.html("");
		$.get('/api/Departments/check', { code: typedCode, info: check_info,  "_": $.now() }, function (t, u, v) {
			$pinborder.removeClass("wait");
			var result = JSON.parse(t);
			if (result.message && result.message == "wrong") {
				$pinborder.addClass("wrong");
				$errorLabel.html("Код введен неправильно");
			} else {
				$pinborder.fadeOut();
				$('#check_info').fadeOut();
				$errorLabel.html("Код принят, спасибо!");
			}
		});
	}
};



$(document).ready(function () {
	var needInfo = $("#check_info").length > 0;
	$(input).on('input', function () {
			checkCode();
		})
		.keypress(function () {
			setTimeout(function () { checkCode(); }, 500);
		})
		.on('focus', function () {
			if (needInfo && $("#check_info").val() == "") {
				$('#check_info').addClass('attention');
			} else
				$('#pin_border').addClass('inputfocused');
		});
	$('#check_info').keypress(function () {
		$('#check_info').removeClass('attention');
	})

	$("#pin_border").click(function () {
		if (needInfo && $("#check_info").val() == "") {
			$('#check_info').addClass('attention').focus();
		} else {
			$("#pin_input").focus();
			$("#pin_border").addClass('inputfocused');
		}
	});

	var args = getUrlVars();
	if (args.code && args.code.length == 4 && !needInfo) {
		$("#pin_border").addClass('inputfocused');
		$('#pin_input').val(args.code);
		checkCode();
	}
});