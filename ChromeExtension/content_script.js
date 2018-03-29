var mercury = new XMLHttpRequest();
mercury.open('GET', 'https://mercury.postlight.com/parser?url=' + window.document.location.href, true);
mercury.setRequestHeader('Content-Type', 'application/json');
mercury.setRequestHeader('x-api-key', '6D2GolgQymAhEgpHsXrKEvwasxWSRSPWB5K1An3x');

mercury.onreadystatechange = function() {
  if(mercury.readyState == 4 && mercury.status == 200) {
    const obj = JSON.parse(mercury.responseText);
    console.log('mecury success', obj);
    mercurySuccess(obj);
  }
}

mercury.send();

function mercurySuccess(data){
  var blueCopy = new XMLHttpRequest();
  blueCopy.open('POST', 'https://copyblue.azurewebsites.net/api/v1/content?redirect=false', true);
  blueCopy.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
  
  blueCopy.onreadystatechange = function() {
    if(blueCopy.readyState == 4 && blueCopy.status == 200) {
      const obj = JSON.parse(blueCopy.responseText);
      console.log('blue copy success', obj);
      blueCopySuccess(obj);
    }
  }

  var postData = getBlueCopyPostData(data);
  
  blueCopy.send(postData);
}

function blueCopySuccess(url){
  document.location.href = url;
}

function getBlueCopyPostData(data) { 
  return 'content=' 
    + encodeURIComponent(`
<html>
  <head>
    <title>%COPYBLUETITLE%</title>
    <link rel="amphtml" href="https://mercury.postlight.com/amp?url=%COPYBLUEURL%">
  </head>
  <body>
    ${data.content}
  </body>
</html>
    `.trim());
}