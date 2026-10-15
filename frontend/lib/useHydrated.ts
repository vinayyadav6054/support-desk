import { useSyncExternalStore } from "react";

const subscribe = () => () => {};

// false while this component is hydrating, true right after
export function useHydrated() {
  return useSyncExternalStore(subscribe, () => true, () => false);
}