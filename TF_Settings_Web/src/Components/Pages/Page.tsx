import { Component } from "react";

interface PageProps {

}
interface PageState {

}

export abstract class Page<TProps extends PageProps = PageProps,
                  TState extends PageState = PageState>
                    extends Component<TProps, TState> {

    render () {
        return (
            <div>
                This is the base Page Render.
                You should not be seeing this.
            </div>
        );
    }
}
