/* 
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */


function regAction() {
    var username = document.getElementById("username");
    var password = document.getElementById("password");

    if (username.value !== null && username.value !== '') {
        if (password.value !== null && password.value !== '') {
            var f = document.getElementById("reg-form");
            f.action = "About.cshtml";
            f.submit();
        } else {
            alert('Invalid Password');
        }
    } else {
        alert('Name is empty');
    }
}