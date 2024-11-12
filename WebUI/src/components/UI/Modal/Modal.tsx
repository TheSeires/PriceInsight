import { FC, ReactNode, useEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import styles from './Modal.module.css';
import { buildClassName } from '../../../utilities/utils';
import { XMarkIcon } from '@heroicons/react/24/outline';

export interface BaseModalProps {
    isOpen: boolean;
    onClose: () => void;
}

export interface ModalProps extends BaseModalProps {
    header: string;
    containerClass?: string;
    children: ReactNode;
}

const portalElement = document.getElementById('portal');

const Modal: FC<ModalProps> = ({
    isOpen = false,
    header,
    containerClass,
    children,
    onClose,
}: ModalProps) => {
    const [isAnimating, setIsAnimating] = useState(false);
    const modalRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (isOpen) {
            setIsAnimating(true);
        } else if (modalRef.current) {
            const animationDuration = parseFloat(
                getComputedStyle(modalRef.current).animationDuration
            );
            const delay = animationDuration
                ? Math.max(animationDuration * 1000 - 25, 0)
                : 400;
            const timer = setTimeout(() => setIsAnimating(false), delay); // Match this with your CSS animation duration
            return () => clearTimeout(timer);
        }
    }, [isOpen]);

    if (!isOpen && !isAnimating) return null;

    if (!portalElement) {
        console.error('Modal component: portalElement is null');
        return null;
    }

    const modalContent = (
        <div
            ref={modalRef}
            className={buildClassName(
                styles['modal-backdrop'],
                'fixed inset-0 z-[1000] flex items-center justify-center p-4',
                isOpen ? 'animate-fade-in' : 'animate-fade-out',
                containerClass
            )}
            tabIndex={0}
        >
            <div
                className="fixed inset-0 bg-black bg-opacity-80"
                onClick={onClose}
            />
            <div
                className={buildClassName(
                    styles['modal'],
                    'relative panel flex flex-col',
                    isOpen ? 'animate-scale-in' : 'animate-scale-out'
                )}
            >
                <div className={styles['header']}>
                    {header}
                    <button
                        className="size-8 p-0 flex items-center justify-center"
                        onClick={onClose}
                    >
                        <XMarkIcon className="size-6" />
                    </button>
                </div>
                <div className={styles['body']}>{children}</div>
            </div>
        </div>
    );

    return createPortal(modalContent, portalElement);
};

export default Modal;
