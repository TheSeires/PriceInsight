import { useCallback, useEffect, useRef, useState } from 'react';

type StateUpdater<T> = (prevState: T) => Promise<T> | T;
type ProvisionalUpdater<T> = (prevState: T) => T;

type UpdateWithProvisional<T> = {
    (
        stateUpdate: StateUpdater<T> | T,
        provisionalUpdate?: ProvisionalUpdater<T> | T
    ): void;
};

const useProvisionalState = <T>(
    initialState: T
): [T, UpdateWithProvisional<T>] => {
    const [currentState, setCurrentState] = useState<T>(initialState);
    const [provisionalState, setProvisionalState] = useState<T>(initialState);
    const pendingRef = useRef<boolean>(false);

    useEffect(() => {
        if (!pendingRef.current) {
            setProvisionalState(currentState);
        }
    }, [currentState]);

    const updateWithProvisional = useCallback<UpdateWithProvisional<T>>(
        (stateUpdate, provisionalUpdate) => {
            pendingRef.current = true;

            const optimisticUpdate = provisionalUpdate ?? stateUpdate;

            setProvisionalState((current) => {
                return typeof optimisticUpdate === 'function'
                    ? (optimisticUpdate as ProvisionalUpdater<T>)(current)
                    : optimisticUpdate;
            });

            Promise.resolve(
                typeof stateUpdate === 'function'
                    ? (stateUpdate as StateUpdater<T>)(currentState)
                    : stateUpdate
            )
                .then((result) => {
                    setCurrentState(result);
                    pendingRef.current = false;
                })
                .catch((error) => {
                    setProvisionalState(currentState);
                    pendingRef.current = false;
                    throw error;
                });
        },
        [currentState]
    );

    return [provisionalState, updateWithProvisional];
};

export default useProvisionalState;
