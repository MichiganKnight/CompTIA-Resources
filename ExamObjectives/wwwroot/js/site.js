document.addEventListener("DOMContentLoaded", function () {
    const checkboxes = document.querySelectorAll(".progress-checkbox");
    const clickableBulletCards = document.querySelectorAll(".progress-bullet-card");
    const clickableSubbulletRows = document.querySelectorAll(".progress-subbullet-row");
    const addButtons = document.querySelectorAll(".checklist-add-button");
    const editButtons = document.querySelectorAll(".checklist-edit-button");
    const deleteButtons = document.querySelectorAll(".checklist-delete-button");
    const saveStatus = document.getElementById("saveStatus");

    const editorModalElement = document.getElementById("checklistEditorModal");
    const editorModal = editorModalElement ? new bootstrap.Modal(editorModalElement) : null;
    const editorForm = document.getElementById("checklistEditorForm");
    const editorTitle = document.getElementById("checklistEditorModalLabel");
    const editorDescription = document.getElementById("checklistEditorModalDescription");
    const saveEditorButton = document.getElementById("saveChecklistEditor");

    const editorFields = {
        mode: document.getElementById("checklistEditorMode"),
        examCode: document.getElementById("checklistEditorExamCode"),
        itemType: document.getElementById("checklistEditorItemType"),
        domainId: document.getElementById("checklistEditorDomainId"),
        objectiveId: document.getElementById("checklistEditorObjectiveId"),
        bulletIndex: document.getElementById("checklistEditorBulletIndex"),
        childIndex: document.getElementById("checklistEditorChildIndex"),
        number: document.getElementById("checklistEditorNumber"),
        title: document.getElementById("checklistEditorTitle"),
        text: document.getElementById("checklistEditorText"),
        weight: document.getElementById("checklistEditorWeight")
    };

    const editorGroups = {
        number: document.getElementById("checklistEditorNumberGroup"),
        title: document.getElementById("checklistEditorTitleGroup"),
        text: document.getElementById("checklistEditorTextGroup"),
        weight: document.getElementById("checklistEditorWeightGroup"),
        objectives: document.getElementById("checklistEditorObjectiveBuilder"),
        bullets: document.getElementById("checklistEditorBulletBuilder"),
        children: document.getElementById("checklistEditorChildBuilder")
    };

    const editorLists = {
        objectives: document.getElementById("editorObjectives"),
        bullets: document.getElementById("editorBullets"),
        children: document.getElementById("editorChildren")
    };

    const addEditorObjectiveButton = document.getElementById("addEditorObjective");
    const addEditorBulletButton = document.getElementById("addEditorBullet");
    const addEditorChildButton = document.getElementById("addEditorChild");

    function showStatus(message, statusClass) {
        if (!saveStatus) {
            return;
        }

        saveStatus.classList.remove("d-none", "text-bg-secondary", "text-bg-success", "text-bg-danger");
        saveStatus.classList.add(statusClass);
        saveStatus.textContent = message;

        if (statusClass !== "text-bg-secondary") {
            setTimeout(function () {
                saveStatus.classList.add("d-none");
            }, 1800);
        }
    }

    function toggleCheckboxFromContainer(container) {
        const checkbox = container.querySelector(".progress-checkbox");

        if (!checkbox) {
            return;
        }

        checkbox.checked = !checkbox.checked;
        checkbox.dispatchEvent(new Event("change", { bubbles: true }));
    }

    function parseJsonDatasetValue(value, fallback) {
        if (!value) {
            return fallback;
        }

        try {
            return JSON.parse(value);
        } catch {
            return fallback;
        }
    }

    function buildChecklistRequestFromElement(element) {
        return {
            examCode: element.dataset.examCode,
            itemType: element.dataset.itemType,
            domainId: Number(element.dataset.domainId || 0),
            objectiveId: Number(element.dataset.objectiveId || 0),
            bulletIndex: Number(element.dataset.bulletIndex || -1),
            childIndex: Number(element.dataset.childIndex || -1),
            number: element.dataset.number || "",
            title: element.dataset.title || "",
            text: element.dataset.text || "",
            weight: Number(element.dataset.weight || 0),
            objectives: parseJsonDatasetValue(element.dataset.objectives, []),
            bullets: parseJsonDatasetValue(element.dataset.bullets, []),
            children: parseJsonDatasetValue(element.dataset.children, [])
        };
    }

    async function sendChecklistRequest(url, request) {
        showStatus("Saving...", "text-bg-secondary");

        try {
            const response = await fetch(url, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(request)
            });

            if (!response.ok) {
                showStatus("Save Failed", "text-bg-danger");
                return;
            }

            const result = await response.json();

            if (!result.success) {
                showStatus("Save Failed", "text-bg-danger");
                return;
            }

            showStatus("Saved", "text-bg-success");
            window.location.reload();
        } catch {
            showStatus("Save Failed", "text-bg-danger");
        }
    }

    function setGroupVisible(group, visible) {
        if (!group) {
            return;
        }

        group.classList.toggle("d-none", !visible);
    }

    function clearEditorLists() {
        Object.values(editorLists).forEach(function (list) {
            if (list) {
                list.innerHTML = "";
            }
        });
    }

    function createButton(text, className) {
        const button = document.createElement("button");
        button.type = "button";
        button.className = className;
        button.textContent = text;
        return button;
    }

    function createInput(value, placeholder, className) {
        const input = document.createElement("input");
        input.className = className || "form-control bg-dark text-body border-secondary";
        input.value = value || "";
        input.placeholder = placeholder || "";
        return input;
    }

    function createTextarea(value, placeholder) {
        const textarea = document.createElement("textarea");
        textarea.className = "form-control bg-dark text-body border-secondary";
        textarea.rows = 2;
        textarea.value = value || "";
        textarea.placeholder = placeholder || "";
        return textarea;
    }

    function createChildEditor(child) {
        const row = document.createElement("div");
        row.className = "checklist-editor-child border border-secondary-subtle rounded-3 p-3";

        const header = document.createElement("div");
        header.className = "d-flex gap-2";

        const text = createInput(child?.Text || child?.text || "", "Sub-bullet text");
        text.dataset.editorField = "child-text";

        const remove = createButton("Remove", "btn btn-outline-danger btn-sm");

        remove.addEventListener("click", function () {
            row.remove();
        });

        header.appendChild(text);
        header.appendChild(remove);
        row.appendChild(header);

        return row;
    }

    function createBulletEditor(bullet) {
        const card = document.createElement("div");
        card.className = "checklist-editor-bullet border border-secondary-subtle rounded-3 p-3";

        const header = document.createElement("div");
        header.className = "d-flex gap-2 mb-3";

        const text = createInput(bullet?.Text || bullet?.text || "", "Bullet text");
        text.dataset.editorField = "bullet-text";

        const addChild = createButton("Add Sub-Bullet", "btn btn-outline-primary btn-sm");
        const remove = createButton("Remove", "btn btn-outline-danger btn-sm");

        const children = document.createElement("div");
        children.className = "d-flex flex-column gap-2 ps-3";
        children.dataset.editorField = "bullet-children";

        addChild.addEventListener("click", function () {
            children.appendChild(createChildEditor({}));
        });

        remove.addEventListener("click", function () {
            card.remove();
        });

        header.appendChild(text);
        header.appendChild(addChild);
        header.appendChild(remove);

        card.appendChild(header);
        card.appendChild(children);

        const existingChildren = bullet?.Children || bullet?.children || [];

        existingChildren.forEach(function (child) {
            children.appendChild(createChildEditor(child));
        });

        return card;
    }

    function createObjectiveEditor(objective) {
        const card = document.createElement("div");
        card.className = "checklist-editor-objective border border-secondary-subtle rounded-3 p-3";

        const header = document.createElement("div");
        header.className = "d-flex flex-column flex-lg-row gap-2 mb-3";

        const number = createInput(objective?.Number || objective?.number || "", "Objective number, example: 1.1");
        number.dataset.editorField = "objective-number";

        const title = createTextarea(objective?.Title || objective?.title || "", "Objective title");
        title.dataset.editorField = "objective-title";

        const remove = createButton("Remove Objective", "btn btn-outline-danger btn-sm align-self-start");

        remove.addEventListener("click", function () {
            card.remove();
        });

        header.appendChild(number);
        header.appendChild(title);
        header.appendChild(remove);

        const bulletHeader = document.createElement("div");
        bulletHeader.className = "d-flex justify-content-between align-items-center mb-2";

        const bulletLabel = document.createElement("span");
        bulletLabel.className = "small text-secondary";
        bulletLabel.textContent = "Bullets";

        const addBullet = createButton("Add Bullet", "btn btn-outline-primary btn-sm");

        const bullets = document.createElement("div");
        bullets.className = "d-flex flex-column gap-2";
        bullets.dataset.editorField = "objective-bullets";

        addBullet.addEventListener("click", function () {
            bullets.appendChild(createBulletEditor({}));
        });

        bulletHeader.appendChild(bulletLabel);
        bulletHeader.appendChild(addBullet);

        card.appendChild(header);
        card.appendChild(bulletHeader);
        card.appendChild(bullets);

        const existingBullets = objective?.Bullets || objective?.bullets || [];

        existingBullets.forEach(function (bullet) {
            bullets.appendChild(createBulletEditor(bullet));
        });

        return card;
    }

    function collectChildrenFromContainer(container) {
        return Array.from(container.querySelectorAll(":scope > .checklist-editor-child"))
            .map(function (childElement) {
                return {
                    Text: childElement.querySelector("[data-editor-field='child-text']").value.trim(),
                    Completed: false,
                    Children: []
                };
            })
            .filter(function (child) {
                return child.Text.length > 0;
            });
    }

    function collectBulletsFromContainer(container) {
        return Array.from(container.querySelectorAll(":scope > .checklist-editor-bullet"))
            .map(function (bulletElement) {
                const childContainer = bulletElement.querySelector("[data-editor-field='bullet-children']");

                return {
                    Text: bulletElement.querySelector("[data-editor-field='bullet-text']").value.trim(),
                    Completed: false,
                    Children: collectChildrenFromContainer(childContainer)
                };
            })
            .filter(function (bullet) {
                return bullet.Text.length > 0;
            });
    }

    function collectObjectivesFromContainer(container) {
        return Array.from(container.querySelectorAll(":scope > .checklist-editor-objective"))
            .map(function (objectiveElement, index) {
                const bulletContainer = objectiveElement.querySelector("[data-editor-field='objective-bullets']");

                return {
                    Id: index + 1,
                    Number: objectiveElement.querySelector("[data-editor-field='objective-number']").value.trim(),
                    Title: objectiveElement.querySelector("[data-editor-field='objective-title']").value.trim(),
                    Completed: false,
                    Bullets: collectBulletsFromContainer(bulletContainer)
                };
            })
            .filter(function (objective) {
                return objective.Number.length > 0 || objective.Title.length > 0;
            });
    }

    function openChecklistEditor(mode, request) {
        if (!editorModal || !editorForm) {
            return;
        }

        editorForm.reset();
        clearEditorLists();

        editorFields.mode.value = mode;
        editorFields.examCode.value = request.examCode || "";
        editorFields.itemType.value = request.itemType || "";
        editorFields.domainId.value = request.domainId || 0;
        editorFields.objectiveId.value = request.objectiveId || 0;
        editorFields.bulletIndex.value = request.bulletIndex ?? -1;
        editorFields.childIndex.value = request.childIndex ?? -1;
        editorFields.number.value = request.number || "";
        editorFields.title.value = request.title || "";
        editorFields.text.value = request.text || "";
        editorFields.weight.value = request.weight || 0;

        const isDomain = request.itemType === "domain";
        const isObjective = request.itemType === "objective";
        const isBullet = request.itemType === "bullet";
        const isChildBullet = request.itemType === "child-bullet";

        setGroupVisible(editorGroups.number, isDomain || isObjective);
        setGroupVisible(editorGroups.title, isDomain || isObjective);
        setGroupVisible(editorGroups.text, isBullet || isChildBullet);
        setGroupVisible(editorGroups.weight, isDomain);
        setGroupVisible(editorGroups.objectives, isDomain);
        setGroupVisible(editorGroups.bullets, isObjective);
        setGroupVisible(editorGroups.children, isBullet || isChildBullet);

        if (isDomain) {
            request.objectives.forEach(function (objective) {
                editorLists.objectives.appendChild(createObjectiveEditor(objective));
            });
        }

        if (isObjective) {
            request.bullets.forEach(function (bullet) {
                editorLists.bullets.appendChild(createBulletEditor(bullet));
            });
        }

        if (isBullet || isChildBullet) {
            request.children.forEach(function (child) {
                editorLists.children.appendChild(createChildEditor(child));
            });
        }

        const action = mode === "add" ? "Add" : "Edit";
        editorTitle.textContent = `${action} ${request.itemType.replace("-", " ")}`;
        editorDescription.textContent = "Manage the item details and nested checklist content in one place.";

        editorModal.show();
    }

    function collectEditorRequest() {
        const itemType = editorFields.itemType.value;

        const request = {
            examCode: editorFields.examCode.value,
            itemType: itemType,
            domainId: Number(editorFields.domainId.value || 0),
            objectiveId: Number(editorFields.objectiveId.value || 0),
            bulletIndex: Number(editorFields.bulletIndex.value || -1),
            childIndex: Number(editorFields.childIndex.value || -1),
            number: editorFields.number.value.trim(),
            title: editorFields.title.value.trim(),
            text: editorFields.text.value.trim(),
            weight: Number(editorFields.weight.value || 0),
            objectives: [],
            bullets: [],
            children: []
        };

        if (itemType === "domain") {
            request.objectives = collectObjectivesFromContainer(editorLists.objectives);
        }

        if (itemType === "objective") {
            request.bullets = collectBulletsFromContainer(editorLists.bullets);
        }

        if (itemType === "bullet" || itemType === "child-bullet") {
            request.children = collectChildrenFromContainer(editorLists.children);
        }

        return request;
    }

    checkboxes.forEach(function (checkbox) {
        checkbox.addEventListener("change", async function () {
            const originalChecked = checkbox.checked;

            showStatus("Saving...", "text-bg-secondary");

            const request = {
                examCode: checkbox.dataset.examCode,
                itemType: checkbox.dataset.itemType,
                domainId: Number(checkbox.dataset.domainId),
                objectiveId: Number(checkbox.dataset.objectiveId || 0),
                bulletIndex: Number(checkbox.dataset.bulletIndex || -1),
                childIndex: Number(checkbox.dataset.childIndex || -1),
                completed: checkbox.checked
            };

            try {
                const response = await fetch("/Home/UpdateProgress", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(request)
                });

                if (!response.ok) {
                    checkbox.checked = !originalChecked;
                    showStatus("Save Failed", "text-bg-danger");
                    return;
                }

                const result = await response.json();

                if (!result.success) {
                    checkbox.checked = !originalChecked;
                    showStatus("Save Failed", "text-bg-danger");
                    return;
                }

                showStatus("Saved", "text-bg-success");
            } catch {
                checkbox.checked = !originalChecked;
                showStatus("Save Failed", "text-bg-danger");
            }
        });
    });

    clickableBulletCards.forEach(function (card) {
        card.addEventListener("click", function (event) {
            if (event.target.closest("input") || event.target.closest("label") || event.target.closest("button")) {
                return;
            }

            toggleCheckboxFromContainer(card);
        });
    });

    clickableSubbulletRows.forEach(function (row) {
        row.addEventListener("click", function (event) {
            if (event.target.closest("input") || event.target.closest("label") || event.target.closest("button")) {
                return;
            }

            toggleCheckboxFromContainer(row);
        });
    });

    addButtons.forEach(function (button) {
        button.addEventListener("click", function () {
            const request = buildChecklistRequestFromElement(button);
            openChecklistEditor("add", request);
        });
    });

    editButtons.forEach(function (button) {
        button.addEventListener("click", function () {
            const request = buildChecklistRequestFromElement(button);
            openChecklistEditor("edit", request);
        });
    });

    deleteButtons.forEach(function (button) {
        button.addEventListener("click", async function () {
            const request = buildChecklistRequestFromElement(button);
            const confirmed = confirm("Delete this checklist item? This cannot be undone.");

            if (!confirmed) {
                return;
            }

            await sendChecklistRequest("/Home/DeleteChecklistItem", request);
        });
    });

    if (addEditorObjectiveButton) {
        addEditorObjectiveButton.addEventListener("click", function () {
            editorLists.objectives.appendChild(createObjectiveEditor({}));
        });
    }

    if (addEditorBulletButton) {
        addEditorBulletButton.addEventListener("click", function () {
            editorLists.bullets.appendChild(createBulletEditor({}));
        });
    }

    if (addEditorChildButton) {
        addEditorChildButton.addEventListener("click", function () {
            editorLists.children.appendChild(createChildEditor({}));
        });
    }

    if (saveEditorButton) {
        saveEditorButton.addEventListener("click", async function () {
            const request = collectEditorRequest();
            const mode = editorFields.mode.value;
            const url = mode === "add" ? "/Home/AddChecklistItem" : "/Home/EditChecklistItem";

            if ((request.itemType === "domain" || request.itemType === "objective") && !request.title) {
                showStatus("Title Required", "text-bg-danger");
                return;
            }

            if ((request.itemType === "bullet" || request.itemType === "child-bullet") && !request.text) {
                showStatus("Text Required", "text-bg-danger");
                return;
            }

            await sendChecklistRequest(url, request);
        });
    }
});