<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebApplicationTest.Default" %>

<!DOCTYPE html>
<html>
<head>
    <title></title>
    <script type="text/javascript" src="jquery-1.10.2.js"></script>
</head>
<body>
    <a href="#" id="get-scan">Get Scan</a>
    <img src="" id="img-scanned" />
    <script>
        $("#get-scan").click(function (evt) {
            evt.preventDefault();

            var url = 'https://localhost:44340/api/Scanner/';
            $.get(url, function (data) {
                $("#img-scanned").attr("src", "data:image/Jpeg;base64,  " + data.GetScanResult);
                console.log('url:', url, 'response:', data);
            });

            /*$.ajax({
                type: "GET",
                url: url,
                contentType: "application/json",
                datatype: "text/json",
                success: function (str) {
                    alert(str);
                },
                error: function (xhr) {
                    alert(xhr.responseText);
                }
            });*/
        });
    </script>
</body>
</html>