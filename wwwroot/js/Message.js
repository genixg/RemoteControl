message = function (ui) {
    this.ui = ui;
};

/*
Типы сообщений
*/
var CMessageType =
    {
        ERROR: 'Ошибка!',
        WARNING: 'Внимание!',
        INFO: 'Информация'
    };

var EMessageType =
    {
        ERROR: 3,
        WARNING: 2,
        INFO: 1
    };

var CTypeNames =
    {
        UNKNOWN: 'неизвестно',
        UPSS: 'СРЕЗ.УП',
        SLICE_SD: 'СРЕЗ.СД',
        SAPFIR: 'Документ АГАТ'
    };

var CButtonsCaptions =
    {
        OK: 'ОК',
        CANCEL: 'Отмена',
        YES: 'Да',
        NO: 'Нет',
        BEGIN: 'Начать'
    };

var CCustomConstants =
    {
        EMPTY_NODE: '0000'
    };


var CEnvironment =
    {
        NewLine: '\n',
        Comma: ',',
        Dot: '.',
        Semicolon: ';',
        Separator: '_'
    };

message.prototype.showWarning = function (title, message, isReload) {
    var me = this.ui;
    Ext.MessageBox.show(
        {
            title:   title,
            msg:     message,
            buttons: Ext.Msg.OK,
            icon:    Ext.MessageBox.WARNING,
            fn: function (buttonId) {
                if (buttonId === 'ok' && isReload) {
                    
                }
            }
        }
    );
};

message.prototype.showError = function (title, message, isReload) {
    var me = this.ui;
    Ext.MessageBox.show(
        {
            title:   title,
            msg:     message,
            buttons: Ext.Msg.OK,
            icon:    Ext.MessageBox.ERROR,
            fn: function (buttonId) {
                if (buttonId === 'ok' && isReload) {
                    
                }
            }
        }
    );
};

message.prototype.showInfo = function (title, message, isReload) {
    var me = this.ui;
    Ext.MessageBox.show(
        {
            title:   title,
            msg:     message,
            buttons: Ext.Msg.OK,
            icon:    Ext.MessageBox.INFO,
            fn: function (buttonId) {
                if (buttonId === 'ok' && isReload) {
                    
                }
            }
        }
    );
};

message.prototype.show = function (title, message, level, isReload) {
    switch (level) {
        case 1:
            this.showInfo(title, message, isReload);
            break;
        case 2:
            this.showWarning(title, message, isReload);
            break;
        case 3:
            this.showError(title, message, isReload);
            break;
        default:
            break;
    }
};

message.prototype.showPopup = function (title, message, level, opId, sticky) {
    this.fly(title, message, level, opId, sticky);
  
};

/*
    Функция вывода сообщения на экран.
    1 - Вывод информационного окна
    2 - Вывод предупреждения.
    3 - Вывод сообщения об ошибке.
*/
message.prototype.fly = function (title, message, level, opId, sticky) {
        var type = '', me = this;
        switch (level) {
            case 1:
                type = 'information';
                break;
            case 2:
                type = 'warning';
                break;
            case 3:
                type = 'error';
                break;
            default:
                type = 'alert';
                break;
        }
		if (sticky && window.stickyNote)
			me.hideSticky();
    window.stickyNote = Noty({
            text:    Ext.String.format('<b>{0}</b><br/>{1}', title, message),
            theme:   'relax',
            type:    type,
            layout:  'bottomRight',
            timeout: sticky ? false : 5000,
            animation:
            {
                open:   me.getAnimation('In'),
                close:  me.getAnimation('Out'),
                easing: 'swing',
                speed:  500
            }
        });
};

message.prototype.hideSticky = function() {
	if (window.stickyNote)
		window.stickyNote.close();
	window.stickyNote = null;
}

message.prototype.confirm = function (title, message, func, nofunc) {
    this.flyConfirm(title, message, func, nofunc);

};


message.prototype.alert = function (title, message, func, btnCaption) {
    var me = this;
    var n = noty(
        {
            text:    Ext.String.format('<b>{0}</b><br/>{1}', title, message),
            theme:   'relax',
            type:    'information',
            layout:  'bottomRight',
            timeout: 15000,
            animation:
                {
                    open:    me.getAnimation('In'),
                    close:   me.getAnimation('Out'),
                    easing:  'swing',
                    speed:   500
                },
            buttons:
            [
	        	{
	        	    addClass: 'btn btn-primary',
	        	    text:     btnCaption,
	        	    onClick: function ($noty) {
	        	        if (func) func();
	        	        $noty.close();
	        	    }
	        	},
	    	    {
	    	        addClass: 'btn btn-danger',
	    	        text:     'Отмена',
	    	        onClick:  function ($noty) {
	    	            $noty.close();
	    	        }
	    	    }
	        ]
        }
    );
};


message.prototype.flyConfirm = function (title, message, func, nofunc) {
    var me = this;
    var n = noty(
    {
        text:     Ext.String.format('<b>{0}</b><br/>{1}', title, message),
        theme:    'relax',
        type:     'information',
        layout:   'bottomRight',
        timeout:  15000,
        animation:
        {
            open:   me.getAnimation('In'),
            close:  me.getAnimation('Out'),
            easing: 'swing',
            speed:  500
        },
        buttons: 
        [
	    	{
	    	    addClass: 'btn btn-primary',
	    	    text:     'Да',
	    	    onClick: function ($noty) {
		            if (func) func();
	    	        $noty.close();
	    	    }
	    	},
	    	{
	    	    addClass: 'btn btn-danger',
	    	    text:     'Нет',
	    	    onClick: function ($noty) {
	    	        if (nofunc) nofunc();
	    	        $noty.close();
	    	    }
	    	}
	    ]
	}
    );
};
message.prototype.getAnimation = function (mode) {
    return Ext.ieVersion <= 9 && Ext.ieVersion !== 0
        ? { height: 'toggle' }
        : 'animated bounce' + mode + 'Right';
};