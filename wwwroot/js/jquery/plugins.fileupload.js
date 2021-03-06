jQuery.extend({
    createUploadIframe: function(id) {
        var frameId = 'jUploadFrame' + id;
        var frame   = $('<iframe id="' + frameId + '" name="' + frameId + '" src="javascript:false" style="display:none" />');

        $(frame).appendTo('body');

        return frame
    },

    createUploadForm: function(id, fileElementId) {
        var formId = 'jUploadForm' + id;
        var fileId = 'jUploadFile' + id;
		var src = $('#' + fileElementId);

        var form   = $('<form action="" method="POST" name="' + formId + '" id="' + formId + '" enctype="multipart/form-data" style="display:none"/>');	
        var input = $(src).clone();
        $(src).attr('id', fileId);
        $(src).before(input);
        $(src).appendTo(form);
        $(form).appendTo('body');

        return form;
    },

    ajaxFileUpload: function(s) {
        s = jQuery.extend({}, jQuery.ajaxSettings, s);
        
        var id      = new Date().getTime()        
		var form    = jQuery.createUploadForm(id, s.fileElementId);
		var frame   = jQuery.createUploadIframe(id);
		var frameId = 'jUploadFrame' + id;
		var formId  = 'jUploadForm' + id;		
        var requestDone = false;
        var xml = {}   

        // Watch for a new set of requests
        if (s.global) {
            if (!jQuery.active++) jQuery.event.trigger("ajaxStart");
            jQuery.event.trigger("ajaxSend", [xml, s]);
        }
        
        var uploadCallback = function() {	
        		
		    var io = document.getElementById(frameId);
            try  {				
			    if (io.contentWindow) {
			        xml.responseText = io.contentWindow.document.body?io.contentWindow.document.body.innerHTML:null;
            	    xml.responseXML = io.contentWindow.document.XMLDocument?io.contentWindow.document.XMLDocument:io.contentWindow.document;
			    } else if(io.contentDocument) {
			        xml.responseText = io.contentDocument.document.body?io.contentDocument.document.body.innerHTML:null;
            	    xml.responseXML = io.contentDocument.document.XMLDocument?io.contentDocument.document.XMLDocument:io.contentDocument.document;
			    }						
            } catch(e) {
			    jQuery.handleError(s, xml, null, e);
		    }
    		
            if (xml) {				
                requestDone = true;
                var status;
                try {
                    status = "success";

                    // process the data (runs the xml through httpData regardless of callback)
                    var data = jQuery.uploadHttpData(xml, s.dataType);    
                    // If a local callback was specified, fire it and pass it the data
                    if (s.success) s.success(data, status);
                    // Fire the global callback
                    if (s.global) jQuery.event.trigger("ajaxSuccess", [xml, s]);
                } catch(e) {
                    status = "error";
                    jQuery.handleError(s, xml, status, e);
                }

                // The request was completed
                if (s.global) jQuery.event.trigger("ajaxComplete", [xml, s]);

                // Handle the global AJAX counter
                if (s.global && ! --jQuery.active)
                    jQuery.event.trigger( "ajaxStop" );

                // Process result
                if (s.complete)
                    s.complete(xml, status);

                jQuery(io).unbind()
                setTimeout(function() {	
                    try {
					    $(io).remove();
					    $(form).remove();	
    			    } catch(e) {
					    jQuery.handleError(s, xml, null, e);
				    }
			    }, 100)

                xml = null
            }
        }
        
        try  {
			var form   = $('#' + formId);
			var ch     = '?'
			var action = s.url;
			
			if (s.data != undefined) {
    			for (param in s.data) {
    			    action += ch + param + '=' + s.data[param]
    			    ch = '&'
	    		}
	    	}

			$(form).attr('action', action);
			$(form).attr('target', frameId);
			if(s.ppost) 
				jQuery.each(s.ppost, function(i, val) {
					$(form).append('<input type="hidden" name="'+i+'" value="'+val+'" />');
				});
            if (form.encoding) {
                form.encoding = 'multipart/form-data';				
            } else {				
                form.enctype = 'multipart/form-data';
            }
            $(form).submit();
        } catch (e) {			
            jQuery.handleError(s, xml, null, e);
        }

        if (window.attachEvent) {
            document.getElementById(frameId).attachEvent('onload', uploadCallback);
        } else {
            document.getElementById(frameId).addEventListener('load', uploadCallback, false);
        } 		
        	
        return {abort: function () {}};	
    },

    uploadHttpData: function( r, type ) {
        var data = !type;

        data = type == "xml" || data ? r.responseXML : r.responseText;

        // If the type is "script", eval it in global context
        if (type == "script")
            jQuery.globalEval(data);

        // Get the JavaScript object, if JSON is used.
        if (type == "json")
            eval("data = " + data);

        // evaluate scripts within html
        if ( type == "html" )
            jQuery("<div>").html(data).evalScripts();

        return data;
    }
})

