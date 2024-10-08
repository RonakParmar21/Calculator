document.addEventListener('DOMContentLoaded', function () {
    const buttons = document.querySelectorAll('.button');
    const handleInput = (action, value) => {
        fetch(`/Home/${action}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ value: value })
        })
            .then(response => response.json())
            .then(data => {
                document.querySelector('.display-1').textContent = data.displayHistory;
                document.querySelector('.display-2').textContent = data.displayResult;
            });
    };

    buttons.forEach(button => {
        button.addEventListener('click', function () {
            const action = this.getAttribute('data-action');
            const value = this.getAttribute('data-value');
            handleInput(action, value);
        });
    });

    document.addEventListener('keydown', function (event) {
        if (event.key >= 0 && event.key <= 9 || event.key === '.') {
            event.preventDefault();
            handleInput('HandleNumber', event.key);
        } else if (event.key === 'Backspace') {
            event.preventDefault();
            handleInput('HandleClearLast');
        } else if (event.key === 'Enter') {
            event.preventDefault();
            handleInput('HandleEqual');
        } else if (event.key === 'Delete' || event.key === 'C' || event.key === "c") {
            event.preventDefault();
            handleInput('HandleClearAll');
        } else if (['+', '-', '*', '/'].includes(event.key)) {
            event.preventDefault();
            handleInput('HandleOperation', event.key);
        }
    });

    document.addEventListener("DOMContentLoaded", function () {
        var isEnabled = Html.Raw(ViewData["IsButtonEnabled"].ToString().ToLower());
        if (!isEnabled) {
            document.getElementById("plusBtn").setAttribute("disabled", "disabled");
            document.getElementById("equalBtn").setAttribute("disabled", "disabled");
            document.getElementById("clearLastBtn").setAttribute("disabled", "disabled");
            document.getElementById("severnNo").setAttribute("disabled", "disabled");
            document.getElementById("eightNo").setAttribute("disabled", "disabled");
            document.getElementById("nineNo").setAttribute("disabled", "disabled");
            document.getElementById("fourNo").setAttribute("disabled", "disabled");
            document.getElementById("fiveNo").setAttribute("disabled", "disabled");
            document.getElementById("sixNo").setAttribute("disabled", "disabled");
            document.getElementById("oneNo").setAttribute("disabled", "disabled");
            document.getElementById("twoNo").setAttribute("disabled", "disabled");
            document.getElementById("oneNo").setAttribute("disabled", "disabled");
            document.getElementById("plusSign").setAttribute("disabled", "disabled");
            document.getElementById("minusSign").setAttribute("disabled", "disabled");
            document.getElementById("multiplicationSign").setAttribute("disabled", "disabled");
            document.getElementById("divideSign").setAttribute("disabled", "disabled");
            document.getElementById("xMark").setAttribute("disabled", "disabled");
            document.getElementById("cAll").setAttribute("disabled", "disabled");
            document.getElementById("zeroNo").setAttribute("disabled", "disabled");
            document.getElementById("dotNo").setAttribute("disabled", "disabled");
        }
    });
});