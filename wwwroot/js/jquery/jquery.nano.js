/* Nano Templates (Tomasz Mazur, Jacek Becela) */

(function ($) {
    $.nano = function (template, data) {
        return template.replace(/\{(\^?[\w\.]*)\}/g, function (str, key) {
            var esc = false;
            if (key.startsWith('^')) {
                key = key.slice(1);
                esc = true;
            }
            var keys = key.split("."), value = data[keys.shift()];
            $.each(keys, function () { if (value === null || value === undefined) return; value = value[this]; });
            return (value === null || value === undefined) ? "" : esc ? escape(value) : value;
        });
    };
})(jQuery);