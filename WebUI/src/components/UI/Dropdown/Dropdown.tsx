import { FC } from 'react';
import { ReactDropdownProps } from 'react-dropdown';
import ReactDropdown from 'react-dropdown';
import 'react-dropdown/style.css';
import './Dropdown.css';

export interface DropdownProps extends ReactDropdownProps {}

const Dropdown: FC<DropdownProps> = (props) => {
    return <ReactDropdown {...props} />;
};

export default Dropdown;
