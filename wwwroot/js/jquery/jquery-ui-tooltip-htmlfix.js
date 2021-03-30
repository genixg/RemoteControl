$.widget("ui.tooltip", $.ui.tooltip, {
    options: {
        content: function () {
            return $(this).prop('title');
        }
    }
});

// � ������ ������ �� ������ ������ ������������ ��������� value � input'�� ����� .attr("value")
// � � jQuery 1.12 ��� �� �������� - ���������� undefined. ������� ��������� ����� ��� �������:
(function ($) {
var origAttrFunc = $.fn.attr;
$.fn.attr = function (attrName) {
	if(arguments.length == 1) {
		if(attrName=="value") {
			var value = this.val();
			if(value)
				return value;
		}
		else if(attrName=="checked") {
			return this.is(":checked");
		}
		else if(attrName=="disabled") {
			return this.is(":disabled");
		}
	}
	else if(arguments.length == 2) {
		if(attrName==="checked" || attrName==="disabled") {
			this.prop(attrName, true);
		}
		else if(attrName=="value") {
			this.val(arguments[1]);
		}
	}
    return origAttrFunc.apply(this, arguments);
};
})(jQuery);