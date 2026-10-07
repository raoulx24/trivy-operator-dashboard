export class RetryPolicyUtils {
  private retryCount = 0;

  nextDelayMs(): number {
    // Always return 1000ms for simplicity
    return 10000;
  }

  resetCounter() {
    this.retryCount = 0;
  }
}
