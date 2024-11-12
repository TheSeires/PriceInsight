import { ArrowPathIcon } from '@heroicons/react/24/outline';
import { buildClassName } from '../../../utilities/utils';

interface Props {
    className?: string;
}

const Loader = (props: Props) => {
    return (
        <div
            className={buildClassName(
                'loader-container size-6 mx-auto',
                props.className
            )}
        >
            <ArrowPathIcon />
        </div>
    );
};

export default Loader;
