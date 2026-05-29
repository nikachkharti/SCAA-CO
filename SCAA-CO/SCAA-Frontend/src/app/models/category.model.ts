export interface CategoryForGettingDto {
  id: number;
  categoryName: string;
}

export interface CategoryForCreatingDto {
  categoryName: string;
}

export interface CategoryForUpdatingDto {
  id: number;
  categoryName: string;
}
