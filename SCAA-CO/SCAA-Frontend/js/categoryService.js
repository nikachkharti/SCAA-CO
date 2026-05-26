import { apiRequest } from "./api.js";

export function getCategories() {
    return apiRequest("/categories");
}

export function getCategoryById(id) {
    return apiRequest(`/categories/${id}`);
}

export function createCategory(categoryName) {
    return apiRequest(
        "/categories",
        "POST",
        {
            categoryName
        }
    );
}

export function updateCategory(id, categoryName) {
    return apiRequest(
        "/categories",
        "PUT",
        {
            id,
            categoryName
        }
    );
}

export function deleteCategory(id) {
    return apiRequest(
        `/categories/${id}`,
        "DELETE"
    );
}