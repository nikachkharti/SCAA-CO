import {
    getCategories,
    createCategory,
    updateCategory,
    deleteCategory
} from "./categoryService.js";

const categoryNameInput = document.getElementById("categoryName");
const categoryIdInput   = document.getElementById("categoryId");
const saveBtn           = document.getElementById("saveBtn");
const updateBtn         = document.getElementById("updateBtn");
const cancelBtn         = document.getElementById("cancelBtn");
const tableBody         = document.getElementById("categoryTableBody");
const statusBox         = document.getElementById("status");

loadCategories();

saveBtn.addEventListener("click", async () => {
    const name = categoryNameInput.value.trim();
    if (!name) {
        showStatus("Category name is required", "error");
        return;
    }

    await run(
        () => createCategory(name),
        "Category created"
    );
});

updateBtn.addEventListener("click", async () => {
    const id   = parseInt(categoryIdInput.value, 10);
    const name = categoryNameInput.value.trim();

    if (!Number.isInteger(id) || id <= 0) {
        showStatus("Invalid category id", "error");
        return;
    }
    if (!name) {
        showStatus("Category name is required", "error");
        return;
    }

    await run(
        () => updateCategory(id, name),
        "Category updated"
    );
});

cancelBtn.addEventListener("click", clearForm);

async function run(action, successMessage) {
    setBusy(true);
    try {
        await action();
        clearForm();
        await loadCategories();
        showStatus(successMessage, "success");
    }
    catch (error) {
        showStatus(error.message, "error");
    }
    finally {
        setBusy(false);
    }
}

async function loadCategories() {
    try {
        const categories = await getCategories();
        renderTable(Array.isArray(categories) ? categories : []);
    }
    catch (error) {
        renderTable([]);
        showStatus(error.message, "error");
    }
}

function renderTable(categories) {
    tableBody.innerHTML = "";

    if (categories.length === 0) {
        const row = document.createElement("tr");
        row.innerHTML = `<td colspan="3" class="empty">No categories yet</td>`;
        tableBody.appendChild(row);
        return;
    }

    for (const category of categories) {
        const row = document.createElement("tr");

        const idCell   = document.createElement("td");
        const nameCell = document.createElement("td");
        const actions  = document.createElement("td");

        idCell.textContent   = category.id;
        nameCell.textContent = category.categoryName;

        const editBtn = document.createElement("button");
        editBtn.className   = "edit";
        editBtn.textContent = "Edit";
        editBtn.addEventListener("click", () => beginEdit(category));

        const deleteBtn = document.createElement("button");
        deleteBtn.className   = "delete";
        deleteBtn.textContent = "Delete";
        deleteBtn.addEventListener("click", () => onDelete(category.id));

        actions.append(editBtn, deleteBtn);
        row.append(idCell, nameCell, actions);
        tableBody.appendChild(row);
    }
}

function beginEdit(category) {
    categoryIdInput.value   = category.id;
    categoryNameInput.value = category.categoryName;
    saveBtn.hidden   = true;
    updateBtn.hidden = false;
    cancelBtn.hidden = false;
    categoryNameInput.focus();
}

async function onDelete(id) {
    if (!confirm("Delete this category?")) return;
    await run(
        () => deleteCategory(id),
        "Category deleted"
    );
}

function clearForm() {
    categoryIdInput.value   = "";
    categoryNameInput.value = "";
    saveBtn.hidden   = false;
    updateBtn.hidden = true;
    cancelBtn.hidden = true;
}

function setBusy(isBusy) {
    saveBtn.disabled   = isBusy;
    updateBtn.disabled = isBusy;
    cancelBtn.disabled = isBusy;
}

let statusTimer = null;
function showStatus(message, kind) {
    if (!statusBox) return;
    statusBox.textContent = message;
    statusBox.className   = `status ${kind}`;
    clearTimeout(statusTimer);
    statusTimer = setTimeout(() => {
        statusBox.textContent = "";
        statusBox.className   = "status";
    }, 4000);
}
