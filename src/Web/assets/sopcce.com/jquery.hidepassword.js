/*
File name: ��ʾ�������û��������롣
Author:guojiaqiu
Date:2015-12-18
url:http://www.sopcce.com
Description: // ��Ҫ������ʾ�������û��������롣
�ı��ı����ԣ�������JQ
*/
(function ($) {
    $.fn.hidePassword = function (options) {
        var s = $.extend($.fn.hidePassword.defaults, options),
        input = $(this);

        $(s.el).bind(s.ev, function () {
            "password" == $(input).attr("type") ?
                $(input).attr("type", "text") :
                $(input).attr("type", "password");
        });
    };
    $.fn.hidePassword.defaults = {
        ev: "click"
    };
}(jQuery));