import { Component } from 'react';

// eslint-disable-next-line @typescript-eslint/no-empty-interface
interface PageProps {}
// eslint-disable-next-line @typescript-eslint/no-empty-interface
interface PageState {}

export abstract class Page<
    TProps extends PageProps = PageProps,
    TState extends PageState = PageState
> extends Component<TProps, TState> {
    render() {
        return (
            <div>
                This is the base Page Render. You should not be seeing this.
            </div>
        );
    }
}
