import { Page } from "./Page";
import { RadioSelector } from "../Controls/RadioSelector";

import '../../Styles/Interactions.css';
import AirPushPreview from '../../Videos/AirPush_Preview.webm';
import TouchPlanePreview from '../../Videos/TouchPlane_Preview.webm';
import HoverPreview from '../../Videos/Hover_Preview.webm';

enum Interaction {
    "AirPush" = "AirPush",
    "TouchPlane" = "TouchPlane",
    "Hover & Hold" = "Hover & Hold"
};

interface interactionsState {
    selectedInteraction: Interaction,
};

export class InteractionsPage extends Page<{}, interactionsState> {
    private videoPaths: Record<string, string> = {
        [Interaction.AirPush.toString()]: AirPushPreview,
        [Interaction.TouchPlane.toString()]: TouchPlanePreview,
        [Interaction["Hover & Hold"].toString()]: HoverPreview,
    };

    constructor(props: {}) {
        super(props);

        let state = {
            // This should come from the current state in the props
            selectedInteraction: Interaction["TouchPlane"],
        };

        this.state = state;
    }

    onInteractionChange(e: React.FormEvent<HTMLInputElement>): void {
        let interaction: Interaction;

        if (!Object.values(Interaction).some((x: string | Interaction) => x.toString() === e.currentTarget.value)) {
            return
        } else {
            interaction = e.currentTarget.value as unknown as Interaction;
            console.log("Selected " + interaction);

            this.setState(() => ({
                selectedInteraction: interaction,
            }));
        }
    }

    resetToDefaults() :void {

    }

    render() {
        let interactions: string[] = [];

        Object.values(Interaction).forEach((x: string | Interaction) => {
            if (typeof x === "string") {
                interactions.push(x.toString());
            }
        });

        return(
            <div className="page">
                <div className="TitleLine">
                    <h1> Interaction Type </h1>
                    <button
                        onClick={this.resetToDefaults.bind(this)}
                        className="tfButton"
                        >
                        <p> Reset to Default </p>
                    </button>
                </div>
                <div className="horizontalContainer">
                    <RadioSelector
                        name="InteractionType"
                        selected={this.state.selectedInteraction.toString()}
                        options={interactions}
                        onChange={this.onInteractionChange.bind(this)} />

                    <video autoPlay loop key={this.state.selectedInteraction} className="InteractionPreview">
                        <source src={this.videoPaths[this.state.selectedInteraction]} type="video/webm"/>
                    </video>
                </div>
            </div>
        );
    }
}