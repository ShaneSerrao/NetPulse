document.addEventListener("DOMContentLoaded", function () {
    console.log(runWalk);  // Check if runWalk is loaded

    // Define the runWalk function
    async function runWalk() {
        const baseOid = document.getElementById("baseOid").value; // Dynamic OID from user input
        const deviceId = window.deviceId || new URLSearchParams(window.location.search).get("deviceId");

        // Validate deviceId and baseOid
        if (!deviceId || !baseOid) {
            console.error("Missing deviceId or baseOid.");
            alert("Please ensure both Device ID and Base OID are provided.");
            return;
        }

        const loadingMessage = document.getElementById("loadingMessage");
        if (loadingMessage) {
            loadingMessage.style.display = "block";
        }

        try {
            const response = await fetch(`/OidBrowser/Walk?deviceId=${deviceId}&baseOid=${baseOid}`);
            const categories = await response.json();

            // Log categories for debugging
            console.log("Categories received from backend:", categories);

            // Check if categories are valid
            if (!categories || Object.keys(categories).length === 0) {
                console.error("No data found for the given OID.");
                alert("No data found for the given OID.");
                return;
            }

            let resultsHtml = "";

            // Loop through categories and their items
            Object.keys(categories).forEach(category => {  // Start of loop
                resultsHtml += `<h3>${category}</h3>`;
                categories[category].forEach(item => {  // Inner loop for items
                    // Check if item is an object and not empty
                    if (typeof item !== 'object' || Object.keys(item).length === 0) {
                        console.warn("Empty or non-object item encountered:", item);
                        return;  // Skip non-object or empty items
                    }

                    // Validate that item has the expected properties
                    if (item.oid && item.name && item.value) {
                        if (typeof item.oid === "string" && typeof item.name === "string" && typeof item.value === "string") {
                            resultsHtml += `<p>${item.oid}: ${item.name} - ${item.value}</p>`;
                        } else {
                            console.warn("Invalid item properties (not string):", item);
                        }
                    } else {
                        console.warn("Incomplete or missing properties:", item);
                    }
                });
            });

            // Display results
            document.getElementById("results").innerHTML = resultsHtml;
        } catch (error) {
            console.error("Error during SNMP walk:", error);
            alert("There was an error fetching data.");
        } finally {
            // Hide the loading message after process is finished
            if (loadingMessage) {
                loadingMessage.style.display = "none";
            }
        }
    }

    // Attach event listener to the Walk button
    const walkButton = document.getElementById("walkButton");
    if (walkButton) {
        walkButton.addEventListener("click", runWalk);
    }
});
