(() => {
    const ratesToUsd = {
        USD: 1.0,
        EUR: 1.09,
        GBP: 1.27,
        CAD: 0.74,
        MXN: 0.059,
        JPY: 0.0067,
        BRL: 0.20,
        AUD: 0.66,
        CHF: 1.12,
        CNY: 0.14,
        INR: 0.012
    };

    function estimate(amount, from, to) {
        if (!Number.isFinite(amount) || amount < 0) {
            return null;
        }

        const fromCode = (from || "").trim().toUpperCase();
        const toCode = (to || "").trim().toUpperCase();
        const fromRate = ratesToUsd[fromCode];
        const toRate = ratesToUsd[toCode];
        if (!fromRate || !toRate) {
            return null;
        }

        const usdAmount = amount * fromRate;
        const converted = usdAmount / toRate;
        return Math.round((converted + Number.EPSILON) * 100) / 100;
    }

    window.attachExpenseEstimator = function attachExpenseEstimator(config) {
        const amountInput = document.querySelector(config.amountSelector);
        const fromInput = document.querySelector(config.fromSelector);
        const toInput = document.querySelector(config.toSelector);
        const outputInput = document.querySelector(config.outputSelector);
        const statusNode = document.querySelector(config.statusSelector);
        if (!amountInput || !fromInput || !toInput || !outputInput) {
            return;
        }

        function renderEstimate() {
            const amount = parseFloat(amountInput.value);
            const result = estimate(amount, fromInput.value, toInput.value);
            if (result === null) {
                if (statusNode) {
                    statusNode.textContent = "Estimate unavailable for that currency code.";
                }
                return;
            }

            outputInput.value = result.toFixed(2);
            if (statusNode) {
                statusNode.textContent = "Rough estimate using static rates.";
            }
        }

        amountInput.addEventListener("input", renderEstimate);
        fromInput.addEventListener("input", renderEstimate);
        fromInput.addEventListener("change", renderEstimate);
        toInput.addEventListener("input", renderEstimate);
        toInput.addEventListener("change", renderEstimate);
        renderEstimate();
    };
})();
