export interface CommonResponse<T = unknown> {
  message: string;
  result: T;
  isSuccess: boolean;
  httpStatusCode: number;
}
