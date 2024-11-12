import { FC } from "react";

const NotFoundPage: FC = () => {
    return (
        <>
            <h3 className="text-center text-lg mt-16 mb-4">This page does not exist</h3>
            <a href="/">Go to home page</a>
        </>
    );
}

export default NotFoundPage;