import { BehaviorSubject, filter, map, Observable, timeout } from 'rxjs';

export class ReactiveMap<TKey, TValue> {
  private subject = new BehaviorSubject<Map<TKey, TValue>>(new Map());

  set(key: TKey, value: TValue): void {
    const updatedMap = new Map(this.subject.value); // clone the existing map to maintain immutability.
    updatedMap.set(key, value);
    this.subject.next(updatedMap);
  }

  getAsync(key: TKey): Observable<TValue> {
    return this.subject.pipe(
      filter((map) => map.has(key)), // filters only when the key exists in the map.
      map((map) => map.get(key)!),
      timeout(30000), // if the key is not found for 30 seconds, it throws an error.
    );
  }

  hasKey(key: TKey): boolean {
    return this.subject.value.has(key);
  }
}
