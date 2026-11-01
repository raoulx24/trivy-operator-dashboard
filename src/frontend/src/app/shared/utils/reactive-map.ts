import { BehaviorSubject, distinctUntilChanged, filter, map, Observable, timeout } from 'rxjs';

export class ReactiveMap<TKey, TValue> {
  private subject = new BehaviorSubject<Map<TKey, TValue>>(new Map());
  // one observable per key, so a template calling getAsync() on every change detection
  // gets the same instance and the async pipe does not resubscribe each time.
  private observables = new Map<TKey, Observable<TValue>>();

  set(key: TKey, value: TValue): void {
    const updatedMap = new Map(this.subject.value); // clone the existing map to maintain immutability.
    updatedMap.set(key, value);
    this.subject.next(updatedMap);
  }

  getAsync(key: TKey): Observable<TValue> {
    let observable = this.observables.get(key);
    if (!observable) {
      observable = this.subject.pipe(
        filter((map) => map.has(key)), // filters only when the key exists in the map.
        map((map) => map.get(key)!),
        distinctUntilChanged(),
        // only the first value is time-bound; once it arrived, the stream stays open for later updates.
        timeout({ first: 30000 }),
      );
      this.observables.set(key, observable);
    }
    return observable;
  }

  hasKey(key: TKey): boolean {
    return this.subject.value.has(key);
  }
}
